// Implementación de ExpoApi sobre cloud-api. Precios, crédito y promociones los decide siempre el servidor.
import {
	ApiError,
	type Cliente,
	type DatosPedido,
	type ExpoApi,
	type LineaCarrito,
	type Pedido,
	type Producto,
	type Promocion,
	type Stand
} from './types';

const KEY = 'cleanexpo.http.v1';

interface Persistido {
	token: string | null;
	cliente: Cliente | null;
	credito: number;
}

type Codigo = ApiError['code'];
const CODIGOS: Codigo[] = ['sin-credito', 'carrito-vacio', 'credenciales', 'no-encontrado', 'invalido', 'mr-no-disponible'];

export class HttpApi implements ExpoApi {
	private s: Persistido = { token: null, cliente: null, credito: 0 };
	private expirada: (() => void)[] = [];

	constructor(
		private base: string,
		private storage: Storage | null = typeof localStorage === 'undefined' ? null : localStorage,
		private fetchFn: typeof fetch = (...a) => fetch(...a)
	) {
		this.base = base.replace(/\/+$/, '');
		try {
			const raw = this.storage?.getItem(KEY);
			if (raw) this.s = JSON.parse(raw);
		} catch {
			/* almacenamiento no disponible: la sesión dura lo que dure la pestaña */
		}
	}

	private save() {
		try {
			this.storage?.setItem(KEY, JSON.stringify(this.s));
		} catch {
			/* idem */
		}
	}

	private async req<T>(method: string, path: string, body?: unknown): Promise<T> {
		const tieneToken = !!this.s.token;
		let res: Response;
		try {
			res = await this.fetchFn(this.base + path, {
				method,
				headers: {
					...(body === undefined ? {} : { 'content-type': 'application/json' }),
					...(this.s.token ? { authorization: `Bearer ${this.s.token}` } : {})
				},
				body: body === undefined ? undefined : JSON.stringify(body)
			});
		} catch {
			throw new ApiError('No hay conexión con el servidor', 'error');
		}
		if (res.status === 204) return undefined as T;
		const data = await res.json().catch(() => null);
		if (res.ok) return data as T;
		// 401 con sesión guardada = el token venció o se revocó; sin sesión es un login fallido.
		if (res.status === 401 && tieneToken && !path.startsWith('/auth/')) this.cerrar(true);
		const code = CODIGOS.find((c) => c === data?.code) ?? 'error';
		throw new ApiError(data?.detail ?? 'Error inesperado del servidor', code);
	}

	private cerrar(avisar: boolean) {
		this.s = { token: null, cliente: null, credito: 0 };
		this.save();
		if (avisar) this.expirada.forEach((cb) => cb());
	}

	/** GET que devuelve undefined si no existe. */
	private async opcional<T>(path: string): Promise<T | undefined> {
		try {
			return await this.req<T>('GET', path);
		} catch (e) {
			if (e instanceof ApiError && e.code === 'no-encontrado') return undefined;
			throw e;
		}
	}

	async login(email: string, password: string) {
		const r = await this.req<{ token: string; cliente: Cliente }>('POST', '/auth/login', { email, password });
		this.s = { token: r.token, cliente: r.cliente, credito: r.cliente.credito };
		this.save();
		return r.cliente;
	}
	sesion() {
		return this.s.token ? this.s.cliente : null;
	}
	logout() {
		const token = this.s.token;
		this.cerrar(false);
		if (token) {
			// Revoca el token en el servidor; si falla, igual vence solo.
			void this.fetchFn(this.base + '/auth/logout', {
				method: 'POST',
				headers: { authorization: `Bearer ${token}` }
			}).catch(() => {});
		}
	}
	creditoDisponible() {
		return this.s.credito;
	}
	async refrescarCredito() {
		const r = await this.req<{ disponible: number }>('GET', '/me/credit');
		this.s.credito = r.disponible;
		this.save();
		return r.disponible;
	}
	alExpirar(cb: () => void) {
		this.expirada.push(cb);
	}

	listStands() {
		return this.req<Stand[]>('GET', '/stands');
	}
	getStand(id: number) {
		return this.opcional<Stand>(`/stands/${id}`);
	}

	listProductos(q: { standId?: number; texto?: string; estado?: Producto['estado'] } = {}) {
		const p = new URLSearchParams();
		if (q.standId !== undefined) p.set('standId', String(q.standId));
		if (q.texto?.trim()) p.set('texto', q.texto.trim());
		if (q.estado) p.set('estado', q.estado);
		const qs = p.toString();
		return this.req<Producto[]>('GET', `/products${qs ? `?${qs}` : ''}`);
	}
	getProducto(id: number) {
		return this.opcional<Producto>(`/products/${id}`);
	}
	listPromociones(standId?: number) {
		return this.req<Promocion[]>('GET', `/promotions${standId === undefined ? '' : `?standId=${standId}`}`);
	}

	getCarrito() {
		return this.req<LineaCarrito[]>('GET', '/cart');
	}
	setLinea(productoId: number, cantidad: number) {
		return this.req<LineaCarrito[]>('PUT', `/cart/items/${productoId}`, { cantidad: Math.floor(cantidad) });
	}
	confirmarLinea(productoId: number, confirmado: boolean) {
		return this.req<LineaCarrito[]>('PUT', `/cart/items/${productoId}/confirm`, { confirmado });
	}
	confirmarTodas(confirmado: boolean) {
		return this.req<LineaCarrito[]>('PUT', '/cart/confirm', { confirmado });
	}

	async colocarPedido(datos: DatosPedido) {
		const pedido = await this.req<Pedido>('POST', '/orders', { datos });
		await this.refrescarCredito().catch(() => {});
		return pedido;
	}
	listPedidos() {
		return this.req<Pedido[]>('GET', '/orders');
	}
	getPedido(id: string) {
		return this.opcional<Pedido>(`/orders/${encodeURIComponent(id)}`);
	}
}
