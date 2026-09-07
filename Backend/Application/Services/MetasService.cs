using AutoMapper;
using Finanzas.Application.DTOs;
using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;

namespace Finanzas.Application.Services
{
    public class MetasService(
        IMetasRepository repo,
        ICuentaRepository cuentaRepo,
        IGastosRepository gastosRepo,
        IMapper mapper)
    {
        private readonly IMetasRepository _repo = repo;
        private readonly ICuentaRepository _cuentaRepo = cuentaRepo;
        private readonly IGastosRepository _gastosRepo = gastosRepo;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<MetaDto>> GetAllAsync(Guid userId)
        {
            var items = await _repo.GetAllAsync(userId);
            return _mapper.Map<IEnumerable<MetaDto>>(items);
        }

        public async Task<MetaDto> CreateAsync(CrearMetaDto dto, Guid userId)
        {
            var meta = _mapper.Map<Meta>(dto);
            meta.UserId = userId;
            if (meta.CuentaId is Guid cId && await _cuentaRepo.GetByIdAsync(cId, userId) is null)
                meta.CuentaId = null;
            var creada = await _repo.CreateAsync(meta);
            return _mapper.Map<MetaDto>(creada);
        }

        public async Task<MetaDto?> EditarAsync(Guid id, EditarMetaDto dto, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return null;

            meta.Nombre        = dto.Nombre;
            meta.MontoObjetivo = dto.MontoObjetivo;
            meta.FechaLimite   = dto.FechaLimite;
            meta.Completada    = meta.MontoActual >= dto.MontoObjetivo;
            meta.CuentaId      = dto.CuentaId is Guid cId && await _cuentaRepo.GetByIdAsync(cId, userId) is not null
                ? cId
                : null;
            var actualizada = await _repo.UpdateAsync(meta);
            return _mapper.Map<MetaDto>(actualizada);
        }

        public async Task<MetaDto?> RegistrarMovimientoAsync(Guid id, CrearMovimientoMetaDto dto, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return null;

            var nuevoSaldo = meta.MontoActual + dto.Monto;
            if (nuevoSaldo < 0)
                throw new InvalidOperationException("No puedes retirar más dinero del disponible en esta meta.");

            string? descripcion = dto.Monto >= 0
                ? await ProcesarAbonoAsync(meta, dto, userId)
                : await ProcesarRetiroAsync(meta, dto, userId);

            meta.MontoActual = nuevoSaldo;
            meta.Completada  = meta.MontoActual >= meta.MontoObjetivo;
            var actualizada = await _repo.UpdateAsync(meta);

            await _repo.AddMovimientoAsync(new MovimientoMeta
            {
                MetaId      = id,
                Monto       = dto.Monto,
                Descripcion = descripcion
            });

            return _mapper.Map<MetaDto>(actualizada);
        }

        // Abono: si se indica una cuenta origen, el dinero sale de ella hacia la
        // cuenta de la meta (si tiene una distinta). Sin cuenta origen es solo un
        // apartado: se asume que el dinero ya está en la cuenta de la meta.
        private async Task<string?> ProcesarAbonoAsync(Meta meta, CrearMovimientoMetaDto dto, Guid userId)
        {
            if (dto.CuentaOrigenId is not Guid origenId) return null;

            var origen = await _cuentaRepo.GetByIdAsync(origenId, userId)
                ?? throw new InvalidOperationException("La cuenta de origen no existe.");

            await _cuentaRepo.AddMovimientoAsync(new MovimientoCuenta
            {
                CuentaId    = origenId,
                Monto       = -dto.Monto,
                Descripcion = $"Abono a meta: {meta.Nombre}"
            });

            if (meta.CuentaId is Guid metaCuentaId && metaCuentaId != origenId)
                await _cuentaRepo.AddMovimientoAsync(new MovimientoCuenta
                {
                    CuentaId    = metaCuentaId,
                    Monto       = dto.Monto,
                    Descripcion = $"Abono a meta: {meta.Nombre}"
                });

            return $"Desde {origen.Nombre}";
        }

        private async Task<string?> ProcesarRetiroAsync(Meta meta, CrearMovimientoMetaDto dto, Guid userId)
        {
            var montoRetiro = -dto.Monto; // dto.Monto es negativo en un retiro

            switch (dto.TipoRetiro?.ToUpperInvariant())
            {
                case "GASTO":
                {
                    var ahora = DateTime.UtcNow;
                    var detalle = string.IsNullOrWhiteSpace(dto.Detalle)
                        ? $"Retiro de meta: {meta.Nombre}"
                        : dto.Detalle.Trim();

                    await _gastosRepo.CreateAsync(new Gasto
                    {
                        Categoria    = string.IsNullOrWhiteSpace(dto.Categoria) ? "Otros" : dto.Categoria.Trim(),
                        Detalle      = detalle.Length > 150 ? detalle[..150] : detalle,
                        Monto        = montoRetiro,
                        Tipo         = TipoGasto.VARIABLE,
                        Mes          = ahora.Month,
                        Anio         = ahora.Year,
                        Fecha        = ahora,
                        UserId       = userId,
                        CuentaId     = meta.CuentaId
                    });

                    return $"Gasto: {detalle}";
                }

                case "TRANSFERENCIA":
                {
                    if (dto.CuentaDestinoId is not Guid destinoId)
                        throw new InvalidOperationException("Elige la cuenta destino de la transferencia.");

                    var destino = await _cuentaRepo.GetByIdAsync(destinoId, userId)
                        ?? throw new InvalidOperationException("La cuenta destino no existe.");

                    await _cuentaRepo.AddMovimientoAsync(new MovimientoCuenta
                    {
                        CuentaId    = destinoId,
                        Monto       = montoRetiro,
                        Descripcion = $"Retiro de meta: {meta.Nombre}"
                    });

                    if (meta.CuentaId is Guid metaCuentaId && metaCuentaId != destinoId)
                        await _cuentaRepo.AddMovimientoAsync(new MovimientoCuenta
                        {
                            CuentaId    = metaCuentaId,
                            Monto       = -montoRetiro,
                            Descripcion = $"Retiro de meta: {meta.Nombre}"
                        });

                    return $"A {destino.Nombre}";
                }

                default:
                    return null;
            }
        }

        public async Task<IEnumerable<MovimientoMetaDto>?> GetMovimientosAsync(Guid id, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return null;

            var movimientos = await _repo.GetMovimientosAsync(id);
            return _mapper.Map<IEnumerable<MovimientoMetaDto>>(movimientos);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var meta = await _repo.GetByIdAsync(id, userId);
            if (meta is null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}
