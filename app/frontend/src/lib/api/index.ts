import { MockApi } from './mock';
import type { ExpoApi } from './types';

// Hoy solo existe el mock. Cuando cloud-api exponga catálogo, precios y carrito,
// se agrega una implementación HTTP de ExpoApi y se elige aquí con VITE_API_MODE=http.
export const api: ExpoApi = new MockApi();
export * from './types';
