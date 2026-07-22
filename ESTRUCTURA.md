# Información del Proyecto

## Resumen de Carpetas y Archivos

```
GestionCompras/
├── Controllers/                    # Controladores MVC
│   ├── HomeController.cs
│   ├── ProductosController.cs
│   ├── OrdenesCompraController.cs
│   └── AuditoriaController.cs
│
├── Views/                          # Vistas Razor
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Productos/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── OrdenesCompra/
│   │   ├── Index.cshtml
│   │   └── Details.cshtml
│   ├── Auditoria/
│   │   └── Index.cshtml
│   └── Shared/
│       └── _Layout.cshtml
│
├── Models/                         # Modelos de datos
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── Proveedor.cs
│   ├── Producto.cs
│   ├── CategoriaProducto.cs
│   ├── OrdenCompra.cs
│   ├── DetalleOrdenCompra.cs
│   ├── RecepcionProducto.cs
│   ├── DetalleRecepcion.cs
│   ├── Auditoria.cs
│   └── MovimientoInventario.cs
│
├── Data/                           # Acceso a datos
│   └── GestionComprasDbContext.cs
│
├── Business/                       # Lógica de negocio
│   ├── Interfaces/
│   │   ├── IAuditoriaService.cs
│   │   ├── IOrdenCompraService.cs
│   │   └── IProductoService.cs
│   └── Services/
│       ├── AuditoriaService.cs
│       ├── OrdenCompraService.cs
│       └── ProductoService.cs
│
├── Scripts/                        # Scripts de BD
│   └── 01_CreateDatabase.sql
│
├── Program.cs
├── appsettings.json
├── GestionCompras.csproj
├── README.md
└── INSTALACION.md
```

## Descripción de Componentes

### Models
- **Usuario**: Usuario del sistema con autenticación
- **Rol**: Roles y permisos
- **Proveedor**: Datos de proveedores
- **Producto**: Catálogo de productos
- **OrdenCompra**: Orden de compra a proveedores
- **RecepcionProducto**: Recepción de productos comprados
- **Auditoria**: Trazabilidad de todas las operaciones

### Controllers
- **ProductosController**: ABM de productos, stock bajo
- **OrdenesCompraController**: Gestión de órdenes de compra
- **AuditoriaController**: Visualización de registros de auditoría

### Services
- **ProductoService**: Lógica de gestión de productos y stock
- **OrdenCompraService**: Gestión de órdenes de compra
- **AuditoriaService**: Registro de todas las operaciones

### Views
Interfaces responsivas con Bootstrap 5:
- Dashboard principal
- Listados de productos y órdenes
- Formularios de creación y edición
- Visualización de auditoría y trazabilidad
