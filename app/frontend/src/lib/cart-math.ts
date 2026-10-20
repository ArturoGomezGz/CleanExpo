import type { LineaCarrito, LineaPedido, Producto, Promocion } from './api/types';

export const IVA = 0.16;

export interface Totales {
	subtotal: number;
	iva: number;
	total: number;
	piezas: number;
}

const r2 = (n: number) => Math.round(n * 100) / 100;

export function totales(lineas: { cantidad: number; unitario: number; iva: boolean }[]): Totales {
	let subtotal = 0;
	let iva = 0;
	let piezas = 0;
	for (const l of lineas) {
		const importe = l.cantidad * l.unitario;
		subtotal += importe;
		if (l.iva) iva += importe * IVA;
		piezas += l.cantidad;
	}
	return { subtotal: r2(subtotal), iva: r2(iva), total: r2(subtotal + iva), piezas };
}

export function totalesCarrito(lineas: LineaCarrito[], productos: Map<number, Producto>): Totales {
	return totales(
		lineas
			.filter((l) => l.confirmado && productos.has(l.productoId))
			.map((l) => {
				const p = productos.get(l.productoId)!;
				return { cantidad: l.cantidad, unitario: p.precio, iva: p.iva };
			})
	);
}

export function totalesPedido(lineas: LineaPedido[]): Totales {
	return totales(lineas);
}

/** Promociones ganadas con las líneas confirmadas. Las no acumulables solo dan una vez; las acumulables, una por cada múltiplo del umbral. */
export function promocionesGanadas(
	lineas: LineaCarrito[],
	productos: Map<number, Producto>,
	promos: Promocion[]
): { promo: Promocion; veces: number }[] {
	const porStand = new Map<number, { piezas: number; monto: number }>();
	for (const l of lineas) {
		const p = productos.get(l.productoId);
		if (!p || !l.confirmado) continue;
		const acc = porStand.get(p.standId) ?? { piezas: 0, monto: 0 };
		acc.piezas += l.cantidad;
		acc.monto += l.cantidad * p.precio;
		porStand.set(p.standId, acc);
	}
	const out: { promo: Promocion; veces: number }[] = [];
	for (const promo of promos) {
		const acc = porStand.get(promo.standId);
		if (!acc) continue;
		const base = promo.tipo === 'piezas' ? acc.piezas : acc.monto;
		const veces = promo.acumulable ? Math.floor(base / promo.umbral) : base >= promo.umbral ? 1 : 0;
		if (veces > 0) out.push({ promo, veces });
	}
	return out;
}
