<script lang="ts">
	import { api } from '#lib/api/index.ts';
	import { money } from '#lib/format.ts';
	const data = Promise.all([api.listPromociones(), api.listStands()]);
</script>

<h1>Promociones</h1>
{#await data then [promos, stands]}
	<div class="grid">
		{#each promos as p (p.id)}
			{@const s = stands.find((x) => x.id === p.standId)}
			<article class="card" style:border-top="4px solid {s?.color}">
				<span class="muted">{s?.nombre}</span>
				<h3>{p.titulo}</h3>
				<p>
					{p.tipo === 'piezas' ? `Al comprar ${p.umbral} piezas` : `Al comprar ${money(p.umbral)}`} →
					<strong>{p.premio}</strong>
				</p>
				<p class="muted">{p.acumulable ? 'Acumulable' : 'Una vez por cliente'}</p>
				<a class="btn secondary" href="/stand/{p.standId}">Ver stand</a>
			</article>
		{/each}
	</div>
{/await}
