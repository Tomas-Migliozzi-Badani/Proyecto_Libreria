// Vue maneja la pantalla; fetch envia las operaciones al controlador sin recargar.
Vue.createApp({
    data() {
        return { elementos: [], buscar: '', pagina: 1, paginas: 1, total: 0,
            formulario: { nombre: '', estado: true }, editando: null,
            cargando: false, guardando: false, mensaje: '', error: false, solicitud: 0 };
    },
    mounted() { this.cargar(1); },
    methods: {
        async solicitar(accion, opciones = {}) {
            const base = document.getElementById('categorias-app').dataset.base;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const respuesta = await fetch(`${base}/${accion}`, {
                ...opciones,
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest', 'RequestVerificationToken': token }
            });
            if (respuesta.status === 401) {
                window.location.assign(document.body.dataset.login + '?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search));
                throw new Error('Inicia sesion para continuar.');
            }
            const datos = await respuesta.json().catch(() => ({}));
            if (!respuesta.ok) throw new Error(datos.mensaje || 'No se pudo completar la operacion. Intenta nuevamente.');
            return datos;
        },
        async cargar(pagina) {
            const solicitud = ++this.solicitud;
            this.cargando = true;
            try {
                const datos = await this.solicitar(`Listar?pagina=${pagina}&buscar=${encodeURIComponent(this.buscar)}`);
                if (solicitud !== this.solicitud) return;
                this.elementos = datos.elementos;
                this.pagina = datos.pagina; this.paginas = datos.paginas; this.total = datos.total;
            } catch (e) {
                if (solicitud === this.solicitud) { this.mensaje = e.message; this.error = true; }
            } finally { if (solicitud === this.solicitud) this.cargando = false; }
        },
        editar(categoria) {
            this.editando = categoria.idCategoria;
            this.formulario = { nombre: categoria.nombre, estado: categoria.estado };
            this.mensaje = ''; document.getElementById('nombre-categoria').focus();
        },
        cancelar() { this.editando = null; this.formulario = { nombre: '', estado: true }; },
        async guardar() {
            if (this.guardando) return;
            if (!this.formulario.nombre.trim()) { this.error = true; this.mensaje = 'El nombre es obligatorio.'; return; }
            this.guardando = true;
            try {
                const datos = await this.solicitar(this.editando ? `Editar?id=${this.editando}` : 'Crear', {
                    method: this.editando ? 'PUT' : 'POST', body: JSON.stringify(this.formulario)
                });
                this.mensaje = datos.mensaje; this.error = false;
                this.cancelar(); await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        },
        async desactivar(categoria) {
            if (this.guardando || !confirm(`¿Desactivar la categoria "${categoria.nombre}"?`)) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(`Desactivar?id=${categoria.idCategoria}`, { method: 'DELETE' });
                this.mensaje = datos.mensaje; this.error = false; await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        }
    }
}).mount('#categorias-app');
