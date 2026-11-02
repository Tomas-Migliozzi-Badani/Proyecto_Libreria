Vue.createApp({
    data() { return { elementos: [], seleccionada: null, pagina: 1, paginas: 1, total: 0,
        cargando: false, ocupado: false, mensaje: '', error: false, solicitud: 0 }; },
    mounted() { this.cargar(1); },
    methods: {
        moneda(valor) { return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor); },
        fecha(valor) { return new Date(valor).toLocaleString('es-AR'); },
        async solicitar(accion, opciones = {}) {
            const raiz = document.getElementById('mis-reservas-app');
            const respuesta = await fetch(`${raiz.dataset.base}/${accion}`, { ...opciones, headers: {
                'X-Requested-With': 'XMLHttpRequest', RequestVerificationToken: raiz.querySelector('input[name="__RequestVerificationToken"]').value } });
            if (respuesta.status === 401) {
                window.location.assign(document.body.dataset.login + '?returnUrl=' + encodeURIComponent(window.location.pathname));
                throw new Error('Inicia sesion para continuar.');
            }
            const datos = await respuesta.json().catch(() => ({}));
            if (!respuesta.ok) throw new Error(datos.mensaje || 'No se pudo completar la operacion.');
            return datos;
        },
        async cargar(pagina) {
            const numero = ++this.solicitud; this.cargando = true;
            try {
                const datos = await this.solicitar(`Listar?pagina=${pagina}`); if (numero !== this.solicitud) return;
                this.elementos = datos.elementos; this.pagina = datos.pagina; this.paginas = datos.paginas; this.total = datos.total;
            } catch (e) { if (numero === this.solicitud) { this.mensaje = e.message; this.error = true; } }
            finally { if (numero === this.solicitud) this.cargando = false; }
        },
        async ver(reserva) {
            if (this.ocupado) return; this.ocupado = true;
            try { this.seleccionada = await this.solicitar(`Detalle?id=${reserva.idReserva}`); }
            catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.ocupado = false; }
        },
        async cancelar(reserva) {
            if (this.ocupado || !confirm(`¿Cancelar tu reserva #${reserva.idReserva}?`)) return;
            this.ocupado = true;
            try {
                const datos = await this.solicitar(`Cancelar?id=${reserva.idReserva}`, { method: 'DELETE' });
                this.mensaje = datos.mensaje; this.error = false; this.seleccionada = null; await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.ocupado = false; }
        }
    }
}).mount('#mis-reservas-app');
