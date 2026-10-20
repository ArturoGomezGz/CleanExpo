import { describe, expect, it } from 'vitest';
import { HttpApi } from './http';
import { ApiError } from './types';

type Llamada = { url: string; init: RequestInit };

function fakeFetch(respuestas: Array<[number, unknown]>, llamadas: Llamada[] = []) {
	const f = (async (url: string, init: RequestInit) => {
		llamadas.push({ url, init });
		const [status, body] = respuestas.shift() ?? [500, null];
		return new Response(status === 204 ? null : JSON.stringify(body), { status });
	}) as unknown as typeof fetch;
	return { f, llamadas };
}

const memoria = (): Storage => {
	const m = new Map<string, string>();
	return {
		getItem: (k) => m.get(k) ?? null,
		setItem: (k, v) => void m.set(k, v),
		removeItem: (k) => void m.delete(k)
	} as Storage;
};

const cliente = { idPersona: 1, nombre: 'Demo', credito: 500 };

describe('HttpApi', () => {
	it('guarda el token al entrar y lo manda en cada llamada', async () => {
		const { f, llamadas } = fakeFetch([
			[200, { token: 'abc', cliente }],
			[200, []]
		]);
		const api = new HttpApi('http://x/api/', memoria(), f);
		await api.login('a@b.c', 'pw');
		expect(api.sesion()?.nombre).toBe('Demo');
		expect(api.creditoDisponible()).toBe(500);
		await api.listStands();
		expect(llamadas[0].url).toBe('http://x/api/auth/login');
		expect((llamadas[1].init.headers as Record<string, string>).authorization).toBe('Bearer abc');
	});

	it('la sesión sobrevive a recargar la página', async () => {
		const st = memoria();
		await new HttpApi('/api', st, fakeFetch([[200, { token: 't', cliente }]]).f).login('a', 'b');
		expect(new HttpApi('/api', st, fakeFetch([]).f).sesion()?.idPersona).toBe(1);
	});

	it('traduce los errores del servidor a ApiError con su código', async () => {
		const { f } = fakeFetch([[409, { detail: 'Crédito insuficiente', code: 'sin-credito' }]]);
		const api = new HttpApi('/api', memoria(), f);
		const e = await api
			.colocarPedido({ idDocumento: 1, usoCfdi: 'G03', direccionId: 1, transporteId: 1, observaciones: '' })
			.catch((x) => x);
		expect(e).toBeInstanceOf(ApiError);
		expect(e.code).toBe('sin-credito');
		expect(e.message).toBe('Crédito insuficiente');
	});

	it('un 401 con sesión cierra la sesión y avisa; en el login solo falla', async () => {
		const st = memoria();
		const { f } = fakeFetch([
			[200, { token: 't', cliente }],
			[401, { detail: 'la sesión expiró', code: 'credenciales' }],
			[401, { detail: 'Correo o contraseña incorrectos', code: 'credenciales' }]
		]);
		const api = new HttpApi('/api', st, f);
		let avisos = 0;
		api.alExpirar(() => avisos++);
		await api.login('a', 'b');
		await api.getCarrito().catch(() => {});
		expect(api.sesion()).toBeNull();
		expect(avisos).toBe(1);
		await api.login('a', 'mal').catch(() => {});
		expect(avisos).toBe(1);
	});

	it('getProducto devuelve undefined si no existe y arma los filtros del catálogo', async () => {
		const { f, llamadas } = fakeFetch([
			[404, { detail: 'producto no existe', code: 'no-encontrado' }],
			[200, []]
		]);
		const api = new HttpApi('/api', memoria(), f);
		expect(await api.getProducto(9)).toBeUndefined();
		await api.listProductos({ standId: 2, texto: ' dolor ', estado: 'nuevo' });
		expect(llamadas[1].url).toBe('/api/products?standId=2&texto=dolor&estado=nuevo');
	});

	it('sin red lanza ApiError en vez de dejar la promesa rota', async () => {
		const api = new HttpApi('/api', memoria(), (async () => {
			throw new TypeError('fetch failed');
		}) as unknown as typeof fetch);
		await expect(api.listStands()).rejects.toBeInstanceOf(ApiError);
	});
});
