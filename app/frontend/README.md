# Frontend de la Expo (SvelteKit)

Tienda web de la Expo: stands, catálogo con precio por cliente, promociones, carrito, pedido en 3 pasos y seguimiento de pedidos.

## Modo demo (sin la API de MR)
Toda la app habla con la interfaz `ExpoApi` (`src/lib/api/types.ts`). Hoy la implementa `MockApi` (`src/lib/api/mock.ts`), con datos ficticios (`seed.ts`) guardados en `localStorage`. No llama a Medicinas Rosario ni a `cloud-api`.

```
npm install
npm run dev          # http://localhost:5173
npm test && npm run check
```
Cuenta demo: `demo@expo.test` / `expo` (distribuidor, crédito $120,000). El estado de un pedido avanza solo con el tiempo (recibido → enviado a MR → parcial → surtido) para ver el flujo completo.

## Pasar a datos reales
Escribir una implementación HTTP de `ExpoApi` y elegirla en `src/lib/api/index.ts`. `cloud-api` hoy solo tiene `POST /orders` y `GET /orders/{id}`; faltan catálogo, precios, crédito, auth y carrito. Los precios siempre los decide el servidor.

## Tema anual
Los componentes solo usan variables CSS (`src/themes/2026.css`). Cada edición = un archivo de tokens nuevo importado en `src/routes/+layout.svelte`.

## Chat proveedor ↔ cliente
Reservado en `src/lib/components/ChatDock.svelte` (botón flotante + interfaz `ChatTransport`). Pendiente decidir Chatwoot vs chat propio.

## Docker
`docker build -t expo-frontend .` y `docker run -p 3000:3000 expo-frontend` (adapter-node). No probado: sin daemon de Docker en esta sesión.
