// La seleccion se conserva en esta pestaña durante el login; no guarda datos personales.
Vue.createApp({
    data() { return { productos: [], cantidades: {}, carrito: [], buscar: '', pagina: 1, paginas: 1,
        total: 0, cargando: false, guardando: false, mensaje: '', error: false, solicitud: 0 }; },
    computed: { totalCarrito() { return this.carrito.reduce((total, linea) => total + (Number(linea.cantidad) || 0) * linea.precio, 0); } },
    watch: { carrito: { deep: true, handler(valor) { try { sessionStorage.setItem('libreria.seleccion', JSON.stringify(valor)); } catch { /* La reserva tambien funciona sin almacenamiento local. */ } } } },
    mounted() {
        try {
            const guardado = JSON.parse(sessionStorage.getItem('libreria.seleccion') || '[]');
            if (Array.isArray(guardado)) this.carrito = guardado.filter(l => l && Number.isInteger(l.idProducto) && l.idProducto > 0
                && Number.isInteger(l.cantidad) && l.cantidad > 0 && l.cantidad <= 1000000 && Number.isFinite(l.precio) && l.precio > 0)
                .slice(0, 50).map(l => ({ idProducto: l.idProducto, cantidad: l.cantidad, precio: l.precio, nombre: String(l.nombre || '').slice(0, 150) }));
        } catch { this.carrito = []; }
        this.cargar(1);
    },
    methods: {
        moneda(valor) { return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(valor); },
        async cargar(pagina) {
            const numero = ++this.solicitud; this.cargando = true;
            try {
                const base = document.getElementById('catalogo-app').dataset.base;
                const r = await fetch(`${base}/Listar?pagina=${pagina}&buscar=${encodeURIComponent(this.buscar)}`);
                if (!r.ok) throw new Error('No se pudo cargar el catalogo. Intenta nuevamente.');
                const datos = await r.json(); if (numero !== this.solicitud) return;
                this.productos = datos.elementos; this.pagina = datos.pagina; this.paginas = datos.paginas; this.total = datos.total;
                for (const p of this.productos) this.cantidades[p.idProducto] ??= 1;
            } catch (e) { if (numero === this.solicitud) { this.error = true; this.mensaje = e.message; } }
            finally { if (numero === this.solicitud) this.cargando = false; }
        },
        agregar(producto) {
            const cantidad = this.cantidades[producto.idProducto];
            const linea = this.carrito.find(l => l.idProducto === producto.idProducto);
            const nuevaCantidad = cantidad + (linea?.cantidad || 0);
            if (!Number.isInteger(cantidad) || cantidad < 1 || nuevaCantidad > producto.stock) {
                this.error = true; this.mensaje = 'Revisa la cantidad: no puede superar el stock disponible.'; return;
            }
            if (!linea && this.carrito.length >= 50) { this.error = true; this.mensaje = 'La reserva admite hasta 50 productos distintos.'; return; }
            if (linea) { linea.cantidad = nuevaCantidad; linea.precio = producto.precio; }
            else this.carrito.push({ idProducto: producto.idProducto, nombre: producto.nombre, precio: producto.precio, cantidad });
            this.error = false; this.mensaje = 'Producto agregado a tu seleccion.';
        },
        async confirmar() {
            if (this.guardando || !this.carrito.length) return;
            if (this.carrito.some(l => !Number.isInteger(l.cantidad) || l.cantidad < 1 || l.cantidad > 1000000)) {
                this.error = true; this.mensaje = 'Todas las cantidades deben ser enteros positivos.'; return;
            }
            this.guardando = true;
            try {
                const raiz = document.getElementById('catalogo-app');
                const respuesta = await fetch(raiz.dataset.confirmar, { method: 'POST', headers: {
                    'Content-Type': 'application/json', 'X-Requested-With': 'XMLHttpRequest',
                    RequestVerificationToken: raiz.querySelector('input[name="__RequestVerificationToken"]').value },
                    body: JSON.stringify({ detalles: this.carrito.map(l => ({ idProducto: l.idProducto, cantidad: l.cantidad })) }) });
                if (respuesta.status === 401) {
                    window.location.assign(document.body.dataset.login + '?returnUrl=' + encodeURIComponent(window.location.pathname));
                    return;
                }
                const datos = await respuesta.json().catch(() => ({}));
                if (!respuesta.ok) throw new Error(datos.mensaje || 'No se pudo confirmar. Revisa tu cuenta e intenta nuevamente.');
                this.carrito = []; this.error = false; this.mensaje = `Reserva #${datos.id}. ${datos.mensaje}`;
                await this.cargar(this.pagina);
            } catch (e) { this.error = true; this.mensaje = e.message; }
            finally { this.guardando = false; }
        }
    }
}).mount('#catalogo-app');
