// Vue maneja la pantalla; fetch envia las operaciones al controlador sin recargar.
Vue.createApp({
    data() {
        return { elementos: [], buscar: '', pagina: 1, paginas: 1, total: 0,
            formulario: { nombre: '', marca: '', precio: 0.01, stock: 0, idCategoria: 0, estado: true }, editando: null,
            cargando: false, guardando: false, mensaje: '', error: false, solicitud: 0, 
            busquedaCategoria: '', categoriaSeleccionada: '', opcionesCategoria: [], buscandoCategorias: false, 
            busquedaRealizada: false, busquedaNumero: 0, temporizador: null };
    },
    mounted() { this.cargar(1); },
    methods: {
        moneda(valor) { return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor); },
        programarBusqueda() { clearTimeout(this.temporizador); this.temporizador = setTimeout(() => this.buscarCategorias(), 300); },
        async buscarCategorias() {
            const numero = ++this.busquedaNumero;
            this.buscandoCategorias = true;
            try {
                const datos = await this.solicitar(`BuscarCategorias?buscar=${encodeURIComponent(this.busquedaCategoria)}`);
                if (numero === this.busquedaNumero) { this.opcionesCategoria = datos; this.busquedaRealizada = true; }
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { if (numero === this.busquedaNumero) this.buscandoCategorias = false; }
        },
        seleccionarCategoria(opcion) {
            ++this.busquedaNumero; clearTimeout(this.temporizador); this.buscandoCategorias = false;
            this.formulario.idCategoria = opcion.idCategoria; this.categoriaSeleccionada = opcion.nombre;
            this.opcionesCategoria = []; this.busquedaRealizada = false;
        },
        async solicitar(accion, opciones = {}) {
            const base = document.getElementById('productos-app').dataset.base;
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
        editar(producto) {
            clearTimeout(this.temporizador); ++this.busquedaNumero; this.buscandoCategorias = false; this.busquedaRealizada = false;
            this.editando = producto.idProducto;
            this.formulario = { nombre: producto.nombre, marca: producto.marca, precio: producto.precio,
                stock: producto.stock, idCategoria: producto.idCategoria, estado: producto.estado };
            this.categoriaSeleccionada = producto.categoriaNombre; this.opcionesCategoria = [];
            this.mensaje = ''; document.getElementById('nombre-producto').focus();
        },
        cancelar() { clearTimeout(this.temporizador); ++this.busquedaNumero; this.buscandoCategorias = false; this.categoriaSeleccionada = ''; this.opcionesCategoria = []; this.busquedaCategoria = ''; this.busquedaRealizada = false; this.editando = null; this.formulario = { nombre: '', marca: '', precio: 0.01, stock: 0, idCategoria: 0, estado: true }; },
        async guardar() {
            if (this.guardando) return;
            if (!this.formulario.nombre.trim()) { this.error = true; this.mensaje = 'El nombre es obligatorio.'; return; }
            if (!this.formulario.idCategoria) { this.error = true; this.mensaje = 'Selecciona una categoria.'; return; }
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
        async desactivar(producto) {
            if (this.guardando || !confirm(`¿Desactivar el producto "${producto.nombre}"?`)) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(`Desactivar?id=${producto.idProducto}`, { method: 'DELETE' });
                this.mensaje = datos.mensaje; this.error = false; await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        }
    }
}).mount('#productos-app');
