Vue.createApp({
    data() { return { elementos: [], formulario: { detalles: [] }, editando: null, soloLectura: false, operadorRegistrado: '',
        busquedas: { Clientes: '', Productos: '' }, seleccionados: { Clientes: '' },
        opciones: { Clientes: [], Productos: [] }, realizadas: {}, numeros: {}, temporizadores: {},
        buscar: '', pagina: 1, paginas: 1, total: 0, cargando: false, guardando: false, mensaje: '', error: false, solicitud: 0 }; },
    computed: { totalFormulario() { return this.formulario.detalles.reduce((total, linea) => total + (Number(linea.cantidad) || 0) * linea.precioUnitario, 0); } },
    mounted() { this.nuevo(); this.cargar(1); },
    methods: {
        moneda(valor) { return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor); },
        fecha(valor) { return new Date(valor).toLocaleString('es-AR'); },
        async solicitar(accion, opciones = {}) {
            const raiz = document.getElementById('reservas-app');
            const respuesta = await fetch(`${raiz.dataset.base}/${accion}`, { ...opciones,
                headers: { 'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest', RequestVerificationToken: raiz.querySelector('input[name="__RequestVerificationToken"]').value } });
            if (respuesta.status === 401) {
                window.location.assign(document.body.dataset.login + '?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search));
                throw new Error('Inicia sesion para continuar.');
            }
            const datos = await respuesta.json().catch(() => ({}));
            if (!respuesta.ok) throw new Error(datos.mensaje || 'No se pudo completar la operacion.');
            return datos;
        },
        limpiarBusquedas() {
            for (const tipo of ['Clientes', 'Productos']) {
                clearTimeout(this.temporizadores[tipo]); this.numeros[tipo] = (this.numeros[tipo] || 0) + 1;
                this.busquedas[tipo] = ''; this.opciones[tipo] = []; this.realizadas[tipo] = false;
            }
        },
        nuevo() {
            this.limpiarBusquedas(); this.editando = null; this.soloLectura = false;
            this.operadorRegistrado = document.getElementById('reservas-app').dataset.operador;
            const manana = new Date(Date.now() + 86400000);
            const local = new Date(manana.getTime() - manana.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
            this.formulario = { idCliente: 0, fechaVencimiento: local, detalles: [] };
            this.seleccionados = { Clientes: '' };
        },
        programar(tipo) { clearTimeout(this.temporizadores[tipo]); this.temporizadores[tipo] = setTimeout(() => this.buscarOpciones(tipo), 300); },
        async buscarOpciones(tipo) {
            const numero = this.numeros[tipo] = (this.numeros[tipo] || 0) + 1;
            try {
                const datos = await this.solicitar(`Buscar${tipo}?buscar=${encodeURIComponent(this.busquedas[tipo])}`);
                if (numero === this.numeros[tipo]) { this.opciones[tipo] = datos; this.realizadas[tipo] = true; }
            } catch (e) { this.mensaje = e.message; this.error = true; }
        },
        seleccionar(tipo, opcion) {
            clearTimeout(this.temporizadores[tipo]); ++this.numeros[tipo];
            this.formulario.idCliente = opcion.id;
            this.seleccionados[tipo] = opcion.nombre; this.opciones[tipo] = []; this.realizadas[tipo] = false;
        },
        agregar(producto) {
            const linea = this.formulario.detalles.find(d => d.idProducto === producto.id);
            if (linea) linea.cantidad++;
            else this.formulario.detalles.push({ idProducto: producto.id, nombre: producto.nombre, cantidad: 1, precioUnitario: producto.precio });
        },
        async cargar(pagina) {
            const numero = ++this.solicitud; this.cargando = true;
            try {
                const datos = await this.solicitar(`Listar?pagina=${pagina}&buscar=${encodeURIComponent(this.buscar)}`);
                if (numero !== this.solicitud) return;
                this.elementos = datos.elementos; this.pagina = datos.pagina; this.paginas = datos.paginas; this.total = datos.total;
            } catch (e) { if (numero === this.solicitud) { this.mensaje = e.message; this.error = true; } }
            finally { if (numero === this.solicitud) this.cargando = false; }
        },
        async ver(item) {
            if (this.guardando) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(`Detalle?id=${item.idReserva}`);
                this.limpiarBusquedas(); this.editando = datos.reserva.idReserva;
                this.soloLectura = datos.reserva.estadoReserva !== 'Pendiente';
                this.formulario = { idCliente: datos.reserva.idCliente,
                    fechaVencimiento: datos.reserva.fechaVencimiento.slice(0, 16), detalles: datos.detalles };
                this.seleccionados = { Clientes: datos.reserva.cliente };
                this.operadorRegistrado = datos.reserva.usuario;
                this.mensaje = ''; document.getElementById('reservas-app').scrollIntoView({ behavior: 'smooth' });
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        },
        async guardar() {
            if (this.guardando || this.soloLectura) return;
            if (!this.formulario.idCliente || !this.formulario.detalles.length) {
                this.mensaje = 'Selecciona cliente y al menos un producto.'; this.error = true; return;
            }
            this.guardando = true;
            try {
                const datos = await this.solicitar(this.editando ? `Editar?id=${this.editando}` : 'Crear', {
                    method: this.editando ? 'PUT' : 'POST', body: JSON.stringify(this.formulario) });
                this.mensaje = datos.mensaje; this.error = false; this.nuevo(); await this.cargar(1);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        },
        async cancelarReserva(item) {
            if (this.guardando || !confirm(`¿Cancelar la reserva #${item.idReserva} y devolver su stock?`)) return;
            this.guardando = true;
            try {
                const datos = await this.solicitar(`Cancelar?id=${item.idReserva}`, { method: 'DELETE' });
                this.mensaje = datos.mensaje; this.error = false;
                if (this.editando === item.idReserva) this.nuevo();
                await this.cargar(this.pagina);
            } catch (e) { this.mensaje = e.message; this.error = true; }
            finally { this.guardando = false; }
        }
    }
}).mount('#reservas-app');
