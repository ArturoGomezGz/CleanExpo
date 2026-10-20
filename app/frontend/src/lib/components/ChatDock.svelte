<!--
	Espacio reservado para el chat proveedor ↔ cliente (requisito del proyecto, aún sin decidir
	entre Chatwoot y chat propio; ver docs/chat-service.md). Cuando se decida, solo se implementa
	ChatTransport y se reemplaza el cuerpo de este componente; el resto de la app no cambia.
-->
<script lang="ts" module>
	export interface ChatTransport {
		/** Abre (o recupera) la conversación entre el cliente en sesión y un proveedor. */
		abrir(standId: number): Promise<{ id: string }>;
		enviar(conversacionId: string, texto: string): Promise<void>;
		suscribir(conversacionId: string, alMensaje: (m: { de: 'cliente' | 'proveedor'; texto: string; ts: string }) => void): () => void;
	}
</script>

<script lang="ts">
	let abierto = $state(false);
</script>

<aside class="dock" aria-label="Chat con proveedores">
	{#if abierto}
		<div class="panel card">
			<div class="row spread">
				<strong>Chat con proveedores</strong>
				<button class="btn secondary" onclick={() => (abierto = false)} aria-label="Cerrar">×</button>
			</div>
			<p class="muted">Próximamente: pregunta a un proveedor por precios, descuentos o dudas del producto.</p>
		</div>
	{:else}
		<button class="btn" onclick={() => (abierto = true)}>💬 Chat</button>
	{/if}
</aside>

<style>
	.dock { position: fixed; right: 1rem; bottom: 1rem; z-index: 10; }
	.panel { width: min(320px, 90vw); }
</style>
