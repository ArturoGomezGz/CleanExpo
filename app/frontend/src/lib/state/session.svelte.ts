import { api, type Cliente } from '#lib/api/index.ts';

class Session {
	cliente = $state<Cliente | null>(api.sesion());
	credito = $state(api.creditoDisponible());

	async login(email: string, password: string) {
		this.cliente = await api.login(email, password);
		this.refrescar();
	}
	logout() {
		api.logout();
		this.cliente = null;
	}
	refrescar() {
		this.credito = api.creditoDisponible();
	}
}

export const session = new Session();
