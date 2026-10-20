// Recorre el flujo completo contra un cloud-api real. Se omite salvo que E2E_API_URL esté definido, p. ej.:
//   (backend) MR_MODE=fake SEED_DEV=true DATABASE_URL=... cargo run -p cloud-api
//   E2E_API_URL=http://localhost:8000 npm test
import { describe, expect, it } from 'vitest';
import { HttpApi } from './http';
import { ApiError } from './types';

const url = process.env.E2E_API_URL;

describe.skipIf(!url)('HttpApi contra cloud-api', () => {
	it('login, catálogo, carrito y pedido', async () => {
		const mem = new Map<string, string>();
		const st = { getItem: (k: string) => mem.get(k) ?? null, setItem: (k: string, v: string) => void mem.set(k, v) } as Storage;
		const api = new HttpApi(url!, st);

		await expect(api.login('demo@expo.test', 'mala')).rejects.toMatchObject({ code: 'credenciales' });
		const cliente = await api.login('demo@expo.test', 'expo');
		expect(cliente.tipo).toBe('distribuidor');
		expect(cliente.direcciones.length).toBeGreaterThan(0);

		const stands = await api.listStands();
		expect(stands.length).toBeGreaterThan(0);
		const productos = await api.listProductos({ standId: stands[0].id });
		expect(productos.length).toBeGreaterThan(0);
		expect(productos[0].precio).toBeLessThanOrEqual(productos[0].precioLista);
		expect((await api.listProductos({ texto: 'capsulas' })).length).toBeGreaterThan(0);
		expect((await api.listPromociones()).length).toBeGreaterThan(0);

		await api.confirmarTodas(true);
		const lineas = await api.setLinea(productos[0].id, 3);
		expect(lineas[0]).toMatchObject({ productoId: productos[0].id, cantidad: 3, confirmado: true });

		const antes = await api.refrescarCredito();
		const pedido = await api.colocarPedido({
			idDocumento: 1,
			usoCfdi: 'G03',
			direccionId: cliente.direcciones[0].id,
			transporteId: cliente.transportes[0].id,
			observaciones: 'prueba e2e'
		});
		expect(pedido.estado).toBe('recibido');
		expect(pedido.lineas[0].unitario).toBe(productos[0].precio);
		expect(api.creditoDisponible()).toBeCloseTo(antes - pedido.total, 2);
		expect(await api.getCarrito()).toEqual([]);
		expect((await api.getPedido(pedido.id))?.folio).toBe(pedido.folio);
		expect((await api.listPedidos())[0].id).toBe(pedido.id);

		await expect(
			api.colocarPedido({ idDocumento: 1, usoCfdi: 'G03', direccionId: 1, transporteId: 1, observaciones: '' })
		).rejects.toBeInstanceOf(ApiError);
		api.logout();
		expect(api.sesion()).toBeNull();
	});
});
