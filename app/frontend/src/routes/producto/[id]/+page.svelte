<script lang="ts">
	import { page } from '$app/state';
	import { api } from '#lib/api/index.ts';
	import ProductCard from '#lib/components/ProductCard.svelte';

	const data = $derived(
		api.getProducto(Number(page.params.id)).then(async (p) => ({
			p,
			stand: p && (await api.getStand(p.standId))
		}))
	);
</script>

{#await data then { p, stand }}
	{#if !p}
		<p>Producto no encontrado. <a href="/">Volver</a></p>
	{:else}
		<p><a href="/stand/{p.standId}">← {stand?.nombre}</a></p>
		<div class="detalle">
			<ProductCard producto={p} color={stand?.color} />
			<div class="card">
				<h2>Detalle</h2>
				<p>{p.presentacion}</p>
				<p class="muted">Código {p.codigo} · {p.iva ? 'Grava IVA' : 'Sin IVA'}</p>
				<p>Pregunta al proveedor sobre descuentos por volumen desde el chat.</p>
			</div>
		</div>
	{/if}
{/await}

<style>
	.detalle { display: grid; gap: 1rem; grid-template-columns: minmax(240px, 360px) 1fr; }
	@media (max-width: 640px) { .detalle { grid-template-columns: 1fr; } }
</style>
