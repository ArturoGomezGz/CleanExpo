import { promocionesGanadas, totales } from '../cart-math';
import { CATALOGO, CLIENTE_DEMO, PROMOCIONES, STANDS } from './seed';
import {
	ApiError,
	type Cliente,
	type DatosPedido,
	type EstadoPedido,
	type ExpoApi,
	type LineaCarrito,
	type Pedido,
	type Producto
} from './types';

const KEY = 'cleanexpo.mock.v1';

interface Persistido {
	cliente: Cliente | null;
	carrito: LineaCarrito[];
	pedidos: Pedido[];
	folio: number;
}

// Los precios del mock imitan la regla del ERP: lista = costo * 1.35; el distribuidor recibe 12 % menos.
const FACTOR_LISTA = 1.35;
const DESC_DISTRIBUIDOR = 0.12;
const r2 = (n: number) => Math.round(n * 100) / 100;

function buildProductos(cliente: Cliente | null): Producto[] {
	let id = 1;
	const out: Producto[] = [];
	for (const stand of STANDS) {
		for (const [nombre, presentacion, costo, iva, estado] of CATALOGO[stand.id]) {
			const lista = r2(costo * FACTOR_LISTA);
			const precio = cliente?.tipo === 'distribuidor' ? r2(lista * (1 - DESC_DISTRIBUIDOR)) : lista;
			out.push({
				id,
				standId: stand.id,
				nombre,
				presentacion,
				codigo: `MR${String(id).padStart(5, '0')}`,
				iva,
				precio,
				precioLista: lista,
				estado
			});
			id++;
		}
	}
	return out;
}

const norm = (s: string) => s.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase();
const wait = () => new Promise((r) => setTimeout(r, 120));

/** El estado del pedido avanza con el tiempo para poder ver el flujo recibido → sincronizado → parcial/surtido. */
export function estadoSimulado(p: Pedido, ahora = Date.now()): Pedido {
	const edad = (ahora - new Date(p.creadoEn).getTime()) / 1000;
	let estado: EstadoPedido = 'recibido';
	if (edad > 20) estado = 'surtido';
	else if (edad > 10) estado = 'parcial';
	else if (edad > 4) estado = 'sincronizado';
	const lineas = p.lineas.map((l, i) => {
		let surtido = 0;
		if (estado === 'surtido') surtido = l.cantidad;
		else if (estado === 'parcial') surtido = i === p.lineas.length - 1 ? Math.floor(l.cantidad / 2) : l.cantidad;
		return { ...l, surtido };
	});
	return { ...p, estado, lineas };
}

export class MockApi implements ExpoApi {
	private s: Persistido = { cliente: null, carrito: [], pedidos: [], folio: 1000 };

	constructor(private storage: Storage | null = typeof localStorage === 'undefined' ? null : localStorage) {
		try {
			const raw = this.storage?.getItem(KEY);
			if (raw) this.s = JSON.parse(raw);
		} catch {
			/* almacenamiento no disponible: el mock funciona en memoria */
		}
	}

	private save() {
		try {
			this.storage?.setItem(KEY, JSON.stringify(this.s));
		} catch {
			/* idem */
		}
	}

	private productos() {
		return buildProductos(this.s.cliente);
	}

	async login(email: string, password: string) {
		await wait();
		if (email.trim().toLowerCase() !== CLIENTE_DEMO.email || password !== 'expo') {
			throw new ApiError('Correo o contraseña incorrectos', 'credenciales');
		}
		this.s.cliente = CLIENTE_DEMO;
		this.save();
		return CLIENTE_DEMO;
	}

	async listStands() {
		await wait();
		return STANDS;
	}
	async getStand(id: number) {
		await wait();
		return STANDS.find((s) => s.id === id);
	}

	async listProductos(q: { standId?: number; texto?: string; estado?: Producto['estado'] } = {}) {
		await wait();
		const t = q.texto ? norm(q.texto) : '';
		return this.productos().filter(
			(p) =>
				(q.standId === undefined || p.standId === q.standId) &&
				(q.estado === undefined || p.estado === q.estado) &&
				(!t || norm(`${p.nombre} ${p.presentacion} ${p.codigo}`).includes(t))
		);
	}
	async getProducto(id: number) {
		await wait();
		return this.productos().find((p) => p.id === id);
	}
	async listPromociones(standId?: number) {
		await wait();
		return PROMOCIONES.filter((p) => standId === undefined || p.standId === standId);
	}

	async getCarrito() {
		return [...this.s.carrito];
	}
	async setLinea(productoId: number, cantidad: number) {
		const i = this.s.carrito.findIndex((l) => l.productoId === productoId);
		if (cantidad <= 0) {
			if (i >= 0) this.s.carrito.splice(i, 1);
		} else if (i >= 0) {
			this.s.carrito[i].cantidad = Math.floor(cantidad);
		} else {
			this.s.carrito.push({ productoId, cantidad: Math.floor(cantidad), confirmado: true });
		}
		this.save();
		return [...this.s.carrito];
	}
	async confirmarLinea(productoId: number, confirmado: boolean) {
		const l = this.s.carrito.find((x) => x.productoId === productoId);
		if (l) l.confirmado = confirmado;
		this.save();
		return [...this.s.carrito];
	}
	async confirmarTodas(confirmado: boolean) {
		for (const l of this.s.carrito) l.confirmado = confirmado;
		this.save();
		return [...this.s.carrito];
	}

	async colocarPedido(datos: DatosPedido) {
		await wait();
		const cliente = this.s.cliente;
		if (!cliente) throw new ApiError('Sesión expirada', 'credenciales');
		const prods = new Map(this.productos().map((p) => [p.id, p]));
		const confirmadas = this.s.carrito.filter((l) => l.confirmado && prods.has(l.productoId));
		if (!confirmadas.length) throw new ApiError('No hay productos confirmados en el carrito', 'carrito-vacio');
		const lineas = confirmadas.map((l) => {
			const p = prods.get(l.productoId)!;
			return { productoId: p.id, nombre: p.nombre, cantidad: l.cantidad, unitario: p.precio, iva: p.iva, surtido: 0 };
		});
		const total = totales(lineas).total;
		const ganadas = promocionesGanadas(confirmadas, prods, PROMOCIONES);
		const usado = this.s.pedidos.reduce((a, p) => a + p.total, 0);
		if (total > cliente.credito - usado) throw new ApiError('Crédito insuficiente para este pedido', 'sin-credito');
		const pedido: Pedido = {
			id: crypto.randomUUID(),
			folio: `EXPO-${++this.s.folio}`,
			creadoEn: new Date().toISOString(),
			estado: 'recibido',
			datos,
			lineas,
			promociones: ganadas.map((g) => (g.veces > 1 ? `${g.veces} × ${g.promo.premio}` : g.promo.premio)),
			total
		};
		this.s.pedidos.unshift(pedido);
		this.s.carrito = this.s.carrito.filter((l) => !l.confirmado);
		this.save();
		return pedido;
	}

	async listPedidos() {
		await wait();
		return this.s.pedidos.map((p) => estadoSimulado(p));
	}
	async getPedido(id: string) {
		await wait();
		const p = this.s.pedidos.find((x) => x.id === id);
		return p && estadoSimulado(p);
	}

	/** Crédito restante del cliente (mock de /clients/{id}/credit). */
	creditoDisponible() {
		const c = this.s.cliente;
		return c ? c.credito - this.s.pedidos.reduce((a, p) => a + p.total, 0) : 0;
	}
	async refrescarCredito() {
		return this.creditoDisponible();
	}
	sesion() {
		return this.s.cliente;
	}
	logout() {
		this.s.cliente = null;
		this.save();
	}
}
