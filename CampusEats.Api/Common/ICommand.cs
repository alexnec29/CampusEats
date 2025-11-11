using MediatR;

namespace CampusEats.Api.Common;

public interface ICommand<out TResponse> : IRequest<TResponse> { }

public interface ICommand : IRequest<Result> { }