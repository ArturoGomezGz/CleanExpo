<script lang="ts">
	import { page } from '$app/state';
	import { api } from '#lib/api/index.ts';
	import ProductCard from '#lib/components/ProductCard.svelte';

	const id = $derived(Number(page.params.id));
	const data = $derived(
		Promise.all([api.getStand(id), api.listProductos({ standId: id }), api.listPromociones(id)])
	);
</script>

{#await data then [stand, productos, promos]}
	{#if !stand}
		<p>Stand no encontrado. <a href="/">Volver</a></p>
	{:else}
		<p><a href="/">← Stands</a></p>
		<header class="card" style:border-left="8px solid {stand.color}">
			<h1>{stand.nombre}</h1>
			<p class="muted">{stand.descripcion}</p>
			{#each stand.anuncios as a}<p><strong>{a.titulo}.</strong> {a.texto}</p>{/each}
			{#each promos as p}<p>🎁 <strong>{p.titulo}</strong>: {p.premio}</p>{/each}
		</header>
		<h2>Productos</h2>
		<div class="grid">
			{#each productos as p (p.id)}<ProductCard producto={p} color={stand.color} />{/each}
		</div>
	{/if}
{/await}

<style>
	header { margin-bottom: 1rem; }
</style>
