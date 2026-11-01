using MediatR;
using SportMarket.Application.Common.Exceptions;
using SportMarket.Application.Common.Interfaces;
using SportMarket.Domain.Entities;

namespace SportMarket.Application.Features.Packages;

public record DeletePackageCommand(Guid Id) : IRequest;

public class DeletePackageCommandHandler : IRequestHandler<DeletePackageCommand>
{
    private readonly IApplicationDbContext _db;

    public DeletePackageCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(DeletePackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _db.Packages.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("پکیج", request.Id);

        _db.Packages.Remove(package);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
