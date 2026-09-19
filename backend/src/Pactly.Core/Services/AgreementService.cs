using Pactly.Core.Domain;
using Pactly.Core.Repositories;

namespace Pactly.Core.Services;

public class InvalidAgreementStateException : Exception
{
    public InvalidAgreementStateException(AgreementStatus current, string attemptedAction)
        : base($"Cannot {attemptedAction} an agreement that is currently {current}.")
    {
    }
}

public class AgreementService
{
    private readonly IAgreementRepository _agreementRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public AgreementService(
        IAgreementRepository agreementRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _agreementRepository = agreementRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    public async Task<Agreement> RequestSignatureAsync(string agreementId)
    {
        var agreement = await GetOrThrowAsync(agreementId);
        if (agreement.Status != AgreementStatus.Draft)
        {
            throw new InvalidAgreementStateException(agreement.Status, "send for signature");
        }

        agreement.Status = AgreementStatus.AwaitingSignature;
        agreement.AuditTrail.Add(new AuditEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Actor = "system",
            Action = "SentForSignature",
            Metadata = "Agreement moved to awaiting signature."
        });

        await _agreementRepository.UpdateAsync(agreement);
        return agreement;
    }

    public async Task<Agreement> SignAsync(string agreementId, string signerEmail, string signatureDataUrl)
    {
        var agreement = await GetOrThrowAsync(agreementId);
        if (agreement.Status != AgreementStatus.AwaitingSignature)
        {
            throw new InvalidAgreementStateException(agreement.Status, "sign");
        }

        var now = DateTimeOffset.UtcNow;

        agreement.SignatureDataUrl = signatureDataUrl;
        agreement.SignedAt = now;
        agreement.Status = AgreementStatus.Signed;
        agreement.AuditTrail.Add(new AuditEntry
        {
            Timestamp = now,
            Actor = signerEmail,
            Action = "Signed",
            Metadata = "Signature captured from checkout flow."
        });

        agreement.Status = AgreementStatus.Executed;
        agreement.ExecutedAt = now.AddSeconds(1);
        agreement.AuditTrail.Add(new AuditEntry
        {
            Timestamp = agreement.ExecutedAt.Value,
            Actor = "Pactly",
            Action = "Executed",
            Metadata = "Countersigned and executed on behalf of Pactly."
        });

        await _agreementRepository.UpdateAsync(agreement);
        await ApplyEntitlementsAsync(agreement);
        return agreement;
    }

    private async Task ApplyEntitlementsAsync(Agreement agreement)
    {
        var order = await _orderRepository.GetByIdAsync(agreement.OrderId);
        if (order is null)
        {
            return;
        }

        order.Status = OrderStatus.Completed;
        await _orderRepository.UpdateAsync(order);

        var user = await _userRepository.GetByIdAsync(agreement.UserId);
        if (user is null)
        {
            return;
        }

        // The plan can be any line: cart order is insertion order, so an
        // add-on added first must not hide the plan from entitlements.
        OrderLine? planLine = null;
        Product? product = null;
        foreach (var line in order.Lines)
        {
            var candidate = await _productRepository.GetByIdAsync(line.ProductId);
            if (candidate is { Type: ProductType.Plan })
            {
                planLine = line;
                product = candidate;
                break;
            }
        }

        if (planLine is null || product is null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        user.CurrentPlanId = product.Id;
        user.CurrentSeats = planLine.Quantity;
        user.CurrentPeriodStart = now;
        user.CurrentPeriodEnd = now.AddMonths(1);

        await _userRepository.UpdateAsync(user);
    }

    private async Task<Agreement> GetOrThrowAsync(string agreementId)
    {
        var agreement = await _agreementRepository.GetByIdAsync(agreementId);
        if (agreement is null)
        {
            throw new KeyNotFoundException($"Agreement '{agreementId}' was not found.");
        }

        return agreement;
    }
}
