# 🧾 API REST de Facturación — .NET 8 + SQLite

API REST para gestión de facturación desarrollada con **ASP.NET Core 8**, **Entity Framework Core**, **SQLite**, autenticación **JWT** y documentación con **Swagger / OpenAPI**.

El proyecto aplica separación de responsabilidades mediante Controllers, Services, Repositories, DTOs y DbContext. Está pensado como proyecto de portafolio y demostración técnica.

> **Importante:** este proyecto simula lógica de facturación, pero **no realiza integración fiscal con SUNAT**, no genera XML UBL, firma digital ni CDR.

---

## 🚀 Tecnologías

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- `PasswordHasher<TUser>` para contraseñas
- Swagger / OpenAPI
- Arquitectura por capas

---

## 🧠 Arquitectura

```text
Controllers
   ↓
Services
   ↓
Repositories
   ↓
DbContext
   ↓
SQLite
```

---

## ✅ Funcionalidades

### 🔐 Seguridad

- Login mediante JWT.
- Contraseñas almacenadas mediante hash.
- Endpoints protegidos con `[Authorize]`.
- Clave JWT y contraseña del administrador fuera del repositorio.
- Swagger configurado para autenticación Bearer.

### 👤 Clientes

- Crear clientes.
- Listar clientes.
- Evitar documentos duplicados.
- Evitar eliminación si el cliente ya tiene facturas asociadas.

### 📦 Productos

- Crear productos.
- Listar productos.
- Actualizar productos.
- Control de stock.
- Evitar códigos duplicados.
- Evitar eliminación si el producto aparece en una factura.

### 🧾 Facturación

- Emitir facturas.
- Generar correlativo automático `F001-000001`.
- Calcular subtotal, IGV y total.
- Descontar stock al emitir.
- Restaurar stock al anular.
- Transacciones de base de datos para emisión y anulación.
- Vista HTML imprimible protegida por JWT.

### 📊 Dashboard

- Resumen general.
- Ventas mensuales.
- Top de clientes.

---

## 📁 Estructura

```text
api-facturacion-dotnet-sqlite/
├── Controllers/
├── Data/
├── DTOs/
├── Entities/
├── Middlewares/
├── Repositories/
├── Services/
├── Program.cs
├── appsettings.json
└── README.md
```

---

## ▶️ Ejecutar localmente

### 1. Clonar el repositorio

```bash
git clone https://github.com/eduardo4563/api-facturacion-dotnet-sqlite.git
cd api-facturacion-dotnet-sqlite
```

### 2. Configurar secretos

La API necesita dos valores que **no deben subirse a GitHub**:

- `JWT_SECRET`: clave usada para firmar los tokens JWT. Debe tener al menos 32 caracteres.
- `ADMIN_PASSWORD`: contraseña del usuario administrador. Debe tener al menos 8 caracteres.

#### PowerShell (Windows)

```powershell
$env:JWT_SECRET="coloca-aqui-una-clave-jwt-de-al-menos-32-caracteres"
$env:ADMIN_PASSWORD="coloca-aqui-una-contrasena-segura"
```

Opcionalmente puedes cambiar el usuario administrador:

```powershell
$env:ADMIN_USERNAME="admin"
```

#### Bash / Linux / macOS

```bash
export JWT_SECRET="coloca-aqui-una-clave-jwt-de-al-menos-32-caracteres"
export ADMIN_PASSWORD="coloca-aqui-una-contrasena-segura"
export ADMIN_USERNAME="admin"
```

### 3. Ejecutar

```bash
dotnet restore
dotnet run
```

La terminal mostrará la URL local de la API. Abre `/swagger` sobre esa URL para usar Swagger UI.

> El archivo `facturacion.db` se crea automáticamente y está ignorado por Git.

---

## 🔥 Flujo de prueba en Swagger

### 1. Iniciar sesión

`POST /api/auth/login`

```json
{
  "username": "admin",
  "password": "LA_MISMA_QUE_CONFIGURASTE_EN_ADMIN_PASSWORD"
}
```

La respuesta contiene el token JWT.

### 2. Autorizar Swagger

Presiona **Authorize** y pega el token JWT. Swagger agrega el esquema Bearer automáticamente.

### 3. Crear cliente

`POST /api/clientes`

```json
{
  "nombre": "Empresa Demo SAC",
  "documento": "20600000001",
  "email": "contacto@demo.com",
  "telefono": "999999999"
}
```

### 4. Crear producto

`POST /api/productos`

```json
{
  "codigo": "SERV-001",
  "nombre": "Servicio de desarrollo backend",
  "precio": 850,
  "stock": 10
}
```

### 5. Emitir factura

`POST /api/facturas/emitir`

```json
{
  "clienteId": 1,
  "detalles": [
    {
      "productoId": 1,
      "cantidad": 1
    }
  ]
}
```

### 6. Consultar facturas

`GET /api/facturas`

### 7. Anular una factura

`PATCH /api/facturas/{id}/anular`

La anulación restaura el stock de los productos incluidos en la factura.

---

## 🔐 Variables para despliegue

En Render, Azure, Railway u otro proveedor, configura como variables de entorno:

```text
JWT_SECRET=<clave-segura-de-al-menos-32-caracteres>
ADMIN_PASSWORD=<contrasena-segura-de-al-menos-8-caracteres>
ADMIN_USERNAME=admin
```

Si despliegas un frontend en otro dominio, agrega su origen permitido mediante la configuración de CORS correspondiente al proveedor.

Nunca subas valores reales de estas variables al repositorio.

---

## 🧪 Estado del proyecto

- ✅ API ejecutable con .NET 8
- ✅ SQLite automático
- ✅ Autenticación JWT
- ✅ Contraseñas con hash
- ✅ Swagger / OpenAPI
- ✅ Clientes y productos
- ✅ Emisión y anulación de facturas
- ✅ Control de stock
- ✅ Transacciones al emitir y anular
- ✅ Dashboard básico
- ✅ Manejo global de errores

---

## 📌 Próximas mejoras

- Migraciones de Entity Framework Core.
- Tests unitarios e integración.
- Docker.
- Refresh tokens.
- Generación real de PDF.
- Frontend en React.
- Integración fiscal con SUNAT como proyecto separado o fase futura.

---

## 👨‍💻 Autor

**Eduardo Jahir Montaño Condemayta**  
Backend / Full Stack Developer  
Lima, Perú

📧 eduardomontanocondemayta@gmail.com
