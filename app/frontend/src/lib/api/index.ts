import { HttpApi } from './http';
import { MockApi } from './mock';
import type { ExpoApi } from './types';

// VITE_API_MODE=http usa cloud-api (VITE_API_URL, por defecto /api detrás del proxy); sin eso, la demo con datos simulados.
export const api: ExpoApi =
	import.meta.env.VITE_API_MODE === 'http'
		? new HttpApi(import.meta.env.VITE_API_URL ?? '/api')
		: new MockApi();
export * from './types';
