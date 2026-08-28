using MediatR;

namespace TaskFlow.Application.Cards.Commands.DeleteCard;

public record DeleteCardCommand(int CardId) : IRequest<bool>;
