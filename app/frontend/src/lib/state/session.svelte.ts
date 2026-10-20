import { api, type Cliente } from '#lib/api/index.ts';

class Session {
	cliente = $state<Cliente | null>(api.sesion());
	credito = $state(api.creditoDisponible());

	constructor() {
		api.alExpirar?.(() => (this.cliente = null));
	}

	async login(email: string, password: string) {
		this.cliente = await api.login(email, password);
		this.credito = api.creditoDisponible();
	}
	logout() {
		api.logout();
		this.cliente = null;
	}
	async refrescar() {
		this.credito = api.creditoDisponible();
		try {
			this.credito = await api.refrescarCredito();
		} catch {
			/* sin red o MR caído: se queda el último crédito conocido */
		}
	}
}

export const session = new Session();
