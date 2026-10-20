<script lang="ts">
	import { page } from '$app/state';
	import { api } from '#lib/api/index.ts';
	import ProductCard from '#lib/components/ProductCard.svelte';

	const q = $derived(page.url.searchParams.get('q') ?? '');
	const resultados = $derived(api.listProductos({ texto: q }));
</script>

<h1>Resultados para “{q}”</h1>
{#await resultados then lista}
	{#if lista.length === 0}
		<p class="muted">Sin resultados.</p>
	{:else}
		<div class="grid">{#each lista as p (p.id)}<ProductCard producto={p} />{/each}</div>
	{/if}
{/await}
