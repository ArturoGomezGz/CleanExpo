import { api, type LineaCarrito, type Producto, type Promocion } from '#lib/api/index.ts';
import { promocionesGanadas, totalesCarrito } from '#lib/cart-math.ts';

class Cart {
	lineas = $state<LineaCarrito[]>([]);
	productos = $state(new Map<number, Producto>());
	promos = $state<Promocion[]>([]);
	listo = $state(false);

	totales = $derived(totalesCarrito(this.lineas, this.productos));
	ganadas = $derived(promocionesGanadas(this.lineas, this.productos, this.promos));
	cantidad = $derived(this.lineas.reduce((a, l) => a + l.cantidad, 0));

	async cargar() {
		const [lineas, productos, promos] = await Promise.all([
			api.getCarrito(),
			api.listProductos(),
			api.listPromociones()
		]);
		this.lineas = lineas;
		this.productos = new Map(productos.map((p) => [p.id, p]));
		this.promos = promos;
		this.listo = true;
	}
	cantidadDe(id: number) {
		return this.lineas.find((l) => l.productoId === id)?.cantidad ?? 0;
	}
	async set(id: number, cantidad: number) {
		this.lineas = await api.setLinea(id, cantidad);
	}
	async confirmar(id: number, v: boolean) {
		this.lineas = await api.confirmarLinea(id, v);
	}
	async confirmarTodas(v: boolean) {
		this.lineas = await api.confirmarTodas(v);
	}
}

export const cart = new Cart();
