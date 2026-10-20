<script lang="ts">
	import type { Producto } from '#lib/api/index.ts';
	import { cart } from '#lib/state/cart.svelte.ts';
	import { money } from '#lib/format.ts';
	import Stepper from './Stepper.svelte';

	let { producto, color }: { producto: Producto; color?: string } = $props();
	const enCarrito = $derived(cart.cantidadDe(producto.id));
	let cantidad = $state(1);
</script>

<article class="card prod" style:border-top="4px solid {color ?? 'var(--color-primary)'}">
	<div class="row spread">
		<span class="muted">{producto.codigo}</span>
		{#if producto.estado === 'nuevo'}<span class="badge ok">Nuevo</span>{/if}
		{#if producto.estado === 'liquidacion'}<span class="badge warn">Liquidación</span>{/if}
	</div>
	<h3><a href="/producto/{producto.id}">{producto.nombre}</a></h3>
	<p class="muted">{producto.presentacion}</p>
	<p class="price">
		{money(producto.precio)}
		{#if producto.precioLista > producto.precio}<s class="muted">{money(producto.precioLista)}</s>{/if}
		<span class="muted">{producto.iva ? '+ IVA' : 'sin IVA'}</span>
	</p>
	<div class="row">
		<Stepper bind:value={cantidad} />
		<button class="btn" onclick={() => cart.set(producto.id, enCarrito + cantidad)}>Agregar</button>
	</div>
	{#if enCarrito}<p class="muted">En carrito: {enCarrito}</p>{/if}
</article>

<style>
	.price { font-size: 1.15rem; font-weight: 700; margin: 0.25rem 0 0.75rem; }
	h3 { font-size: 1rem; }
</style>
