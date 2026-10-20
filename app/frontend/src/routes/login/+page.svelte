<script lang="ts">
	import { goto } from '$app/navigation';
	import { session } from '#lib/state/session.svelte.ts';

	let email = $state('demo@expo.test');
	let password = $state('expo');
	let error = $state('');
	let cargando = $state(false);

	async function entrar(e: SubmitEvent) {
		e.preventDefault();
		cargando = true;
		error = '';
		try {
			await session.login(email, password);
			goto('/');
		} catch (err) {
			error = err instanceof Error ? err.message : 'No se pudo iniciar sesión';
		} finally {
			cargando = false;
		}
	}
</script>

<div class="card login">
	<h1>Expo Medicinas Rosario 2026</h1>
	<p class="muted">Entra con tu cuenta de cliente. Modo demostración: ya está llenada con la cuenta de prueba.</p>
	<form onsubmit={entrar}>
		<div class="field"><label for="e">Correo</label><input id="e" type="email" bind:value={email} required /></div>
		<div class="field"><label for="p">Contraseña</label><input id="p" type="password" bind:value={password} required /></div>
		{#if error}<p class="error" role="alert">{error}</p>{/if}
		<button class="btn" disabled={cargando}>{cargando ? 'Entrando…' : 'Entrar'}</button>
	</form>
</div>

<style>
	.login { max-width: 420px; margin: 3rem auto; }
</style>
