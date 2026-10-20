<script lang="ts">
	import { api } from '#lib/api/index.ts';
	const stands = api.listStands();
	const promos = api.listPromociones();
</script>

<section class="hero card">
	<h1>Expo Medicinas Rosario 2026</h1>
	<p>El partido de las grandes oportunidades comienza aquí.</p>
</section>

{#await promos then ps}
	{#if ps.length}<p><a href="/promociones">🎁 Hay {ps.length} promociones activas →</a></p>{/if}
{/await}

<h2>Stands</h2>
{#await stands}
	<p class="muted">Cargando…</p>
{:then lista}
	<div class="grid">
		{#each lista as s (s.id)}
			<a class="card stand" href="/stand/{s.id}" style:border-top="6px solid {s.color}">
				<div class="logo" style:background={s.color}>{s.nombre.slice(0, 2).toUpperCase()}</div>
				<h3>{s.nombre}</h3>
				<p class="muted">{s.descripcion}</p>
			</a>
		{/each}
	</div>
{/await}

<style>
	.hero { margin-bottom: 1rem; background: var(--color-primary); color: var(--color-on-primary); }
	.hero p { margin: 0; }
	.stand { color: inherit; display: block; }
	.stand:hover { text-decoration: none; transform: translateY(-2px); }
	.logo { width: 3.2rem; height: 3.2rem; border-radius: 50%; color: #fff; display: grid; place-items: center; font-weight: 700; margin-bottom: 0.5rem; }
</style>
