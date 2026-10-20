<script lang="ts">
	import '../themes/base.css';
	import '../themes/2026.css';
	import favicon from '#lib/assets/favicon.svg';
	import { goto } from '$app/navigation';
	import { page } from '$app/state';
	import { session } from '#lib/state/session.svelte.ts';
	import { cart } from '#lib/state/cart.svelte.ts';
	import { money } from '#lib/format.ts';
	import ChatDock from '#lib/components/ChatDock.svelte';
	import type { LayoutProps } from './$types';

	let { children }: LayoutProps = $props();
	let q = $state('');

	const publica = $derived(page.url.pathname === '/login' || page.url.pathname.startsWith('/ayuda'));

	$effect(() => {
		if (!session.cliente && !publica) goto('/login');
		else if (session.cliente && !cart.listo) cart.cargar();
	});

	function buscar(e: SubmitEvent) {
		e.preventDefault();
		goto(`/buscar?q=${encodeURIComponent(q)}`);
	}
	function salir() {
		session.logout();
		cart.listo = false;
		goto('/login');
	}
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
	<title>Expo Medicinas Rosario 2026</title>
</svelte:head>

{#if session.cliente || publica}
	<header class="top">
		<div class="container row spread">
			<a class="brand" href="/">Expo MR 2026</a>
			{#if session.cliente}
				<form class="search" onsubmit={buscar}>
					<input type="search" placeholder="Buscar productos…" bind:value={q} aria-label="Buscar" />
				</form>
				<nav class="row">
					<a href="/">Stands</a>
					<a href="/promociones">Promociones</a>
					<a href="/pedidos">Mis pedidos</a>
					<a href="/carrito" class="btn secondary">Carrito ({cart.cantidad})</a>
					<span class="muted who" title="Crédito disponible">{session.cliente.nombre} · {money(session.credito)}</span>
					<button class="btn secondary" onclick={salir}>Salir</button>
				</nav>
			{/if}
		</div>
	</header>
	<main class="container">
		{@render children()}
	</main>
	{#if session.cliente}<ChatDock />{/if}
{/if}

<style>
	.top { background: var(--color-primary); color: var(--color-on-primary); }
	.top :global(a), .top .muted { color: var(--color-on-primary); }
	.top :global(.btn.secondary) { border-color: var(--color-on-primary); }
	.brand { font-weight: 700; font-size: 1.2rem; }
	.search { flex: 1; min-width: 160px; max-width: 360px; }
	.who { font-size: 0.8rem; }
</style>
