// Clientes y Usuarios comparten la misma mecanica de formularios y paginado.
Vue.createApp({
    data() { return { elementos: [], formulario: {}, editando: null, buscar: '', pagina: 1,
        paginas: 1, total: 0, cargando: false, guardando: false, mensaje: '', error: false, solicitud: 0, buscarCliente: '', clienteNombre: '', clientesDisponibles: [] }; },
    mounted() { this.cancelar(); this.cargar(1); },
    methods: {
        async buscarVinculos() {
            try { this.clientesDisponibles = await this.solicitar(`BuscarClientes?buscar=${encodeURIComponent(this.buscarCliente)}`); }
            catch (e) { this.mensaje = e.message; this.error = true; }
        },
        vincular(opcion) { this.formulario.idCliente = opcion.id; this.clienteNombre = opcion.nombre; this.clientesDisponibles = []; },
        async solicitar(accion, opciones = {}) {
            const raiz = document.getElementById('personas-app');
            const respuesta = await fetch(`${raiz.dataset.base}/${accion}`, { ...opciones,
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest',
                    RequestVerificationToken: raiz.querySelector('input[name="__RequestVerificationToken"]').value } });
            if (respuesta.status === 401) {
                window.location.assign(document.body.dataset.login + '?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search));
                throw new Error('Inicia sesion para continuar.');
            }
            const datos = await respuesta.json().catch(() => ({}));
            if (!respuesta.ok) throw new Error(datos.mensaje || 'No se pudo completar la operacion.');
            return datos;
        },
        cancelar() { this.buscarCliente = ''; this.clienteNombre = ''; this.clientesDisponibles = []; this.editando = null; this.formulario = { nombre: '', apellido: '', email: '',
            dni: '', telefono: '', password: '', rol: 'Empleado', idCliente: null, estado: true }; },
        editar(item) { this.clienteNombre = item.clienteNombre || ''; this.clientesDisponibles = []; this.editando = item.id; this.formulario = { ...item, password: '' }; this.mensaje = ''; document.getElementById('nombre').focus(); },
        async cargar(pagina) {
            const numero = ++this.solicitud; this.cargando = true;
            try {
                const datos = await this.solicitar(`Listar?pagina=${pagina}&buscar=${encodeURIComponent(this.buscar)}`);
                if (numero !== this.solicitud) return;
                this.elementos = datos.elementos; this.pagina = datos.pagina; this.paginas = datos.paginas; this.total = datos.total;
            } catch (e) { if (numero === this.solicitud) { this.mensaje = e.message; this.error = true; } }
            finally { if (numero === this.solicitud) this.cargando = false; }
        },
        async guardar() {
            if (this.guardando) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(this.editando ? `Editar?id=${this.editando}` : 'Crear', {
                    method: this.editando ? 'PUT' : 'POST', body: JSON.stringify({ ...this.formulario, password: this.formulario.password || null }) });
                this.mensaje = datos.mensaje; this.error = false; this.cancelar(); await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        },
        async eliminar(item) {
            if (this.guardando || !confirm(`¿Dar de baja a ${item.nombre} ${item.apellido}?`)) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(`Eliminar?id=${item.id}`, { method: 'DELETE' });
                this.mensaje = datos.mensaje; this.error = false;
                if (this.editando === item.id) this.cancelar();
                await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        }
    }
}).mount('#personas-app');
