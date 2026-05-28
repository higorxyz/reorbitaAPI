using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reorbita.Api.Domain.Interfaces;
using Reorbita.Api.Domain.Structs;
using Reorbita.Api.Models.Requests;
using Reorbita.Api.Models.Responses;

namespace Reorbita.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IServicoAutenticacao _servicoAutenticacao;

    public AuthController(IServicoAutenticacao servicoAutenticacao)
    {
        _servicoAutenticacao = servicoAutenticacao;
    }

    [HttpPost("token")]
    public IActionResult GerarTokenJwt([FromBody] SolicitarTokenJwtRequest request)
    {
        var solicitacaoTokenAcesso = new SolicitacaoTokenAcesso(
            request.UsuarioId,
            request.Operadora,
            request.NivelAcesso,
            request.MfaHabilitado,
            request.CodigoAcesso);

        var resultado = _servicoAutenticacao.GerarToken(solicitacaoTokenAcesso);
        if (!resultado.Sucesso)
        {
            return StatusCode(resultado.CodigoHttp, new ApiResponse<object>
            {
                Sucesso = false,
                Mensagem = resultado.Mensagem,
                CodigoErro = resultado.CodigoErro
            });
        }

        var tokenAcesso = resultado.Dados!;

        return StatusCode(resultado.CodigoHttp, new ApiResponse<TokenJwtResponse>
        {
            Sucesso = true,
            Mensagem = resultado.Mensagem,
            Dados = new TokenJwtResponse
            {
                AccessToken = tokenAcesso.AccessToken,
                TokenType = tokenAcesso.TokenType,
                ExpiraEmUtc = tokenAcesso.ExpiraEmUtc,
                ExpiresInSeconds = tokenAcesso.ExpiresInSeconds
            }
        });
    }
}
