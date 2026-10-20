import { describe, expect, it } from 'vitest';
import { promocionesGanadas, totales, totalesCarrito } from './cart-math';
import type { Producto, Promocion } from './api/types';

const prod = (id: number, standId: number, precio: number, iva = true): Producto => ({
	id,
	standId,
	nombre: `P${id}`,
	presentacion: '',
	codigo: '',
	iva,
	precio,
	precioLista: precio,
	estado: 'activo'
});

describe('totales', () => {
	it('suma IVA solo a productos gravados', () => {
		const t = totales([
			{ cantidad: 2, unitario: 100, iva: true },
			{ cantidad: 1, unitario: 50, iva: false }
		]);
		expect(t).toEqual({ subtotal: 250, iva: 32, total: 282, piezas: 3 });
	});
});

describe('carrito', () => {
	const productos = new Map([prod(1, 10, 100), prod(2, 10, 20)].map((p) => [p.id, p]));

	it('ignora líneas sin confirmar', () => {
		const t = totalesCarrito(
			[
				{ productoId: 1, cantidad: 1, confirmado: true },
				{ productoId: 2, cantidad: 5, confirmado: false }
			],
			productos
		);
		expect(t.subtotal).toBe(100);
	});

	it('promociones: acumulable por múltiplos, no acumulable una vez', () => {
		const promos: Promocion[] = [
			{ id: 1, standId: 10, titulo: 'a', tipo: 'piezas', umbral: 5, premio: 'x', acumulable: true },
			{ id: 2, standId: 10, titulo: 'b', tipo: 'monto', umbral: 100, premio: 'y', acumulable: false }
		];
		const r = promocionesGanadas([{ productoId: 1, cantidad: 12, confirmado: true }], productos, promos);
		expect(r.map((x) => [x.promo.id, x.veces])).toEqual([
			[1, 2],
			[2, 1]
		]);
	});
});
