using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sistema_Biblioteca;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext <BibliotecaDbContext>(options => options.UseInMemoryDatabase("Biblioteca"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

/*app.MapGet("/api/livros", (BibliotecaDbContext banco) =>
{
    // Busca a lista de livros que está no banco e responde com Sucesso (Status 200)
    return Results.Ok(banco.Livros.ToList());
});*/

app.MapPost("/api/livros", (Livro novoLivro, BibliotecaDbContext banco) =>
{
    if (string.IsNullOrWhiteSpace(novoLivro.Titulo) || string.IsNullOrWhiteSpace(novoLivro.Autor) || novoLivro.Autor.Trim().ToLower() == "string" || novoLivro.Titulo.Trim().ToLower() == "string")
    {
        return Results.BadRequest(new { mensagem = "O titulo e o autor são obrigatórios!" });
    }
    if (novoLivro.AnoLancamento <= 0)
    {
        return Results.BadRequest(new { mensagem = "Digite um ano de lançamento válido!" });
    }

    banco.Livros.Add(novoLivro);
    banco.SaveChanges();

    return Results.Created($"api/livros/{novoLivro.Id}", novoLivro);
});

app.MapDelete("api/livros/{id}", (int id, BibliotecaDbContext banco) =>
{
    var livro = banco.Livros.Find(id);

    if (livro == null)
    {
        return Results.NotFound(new
        {
            mensagem = "Livro não encontrado!"
        });
    }

    banco.Livros.Remove(livro);
    banco.SaveChanges();

    return Results.Ok(new
    {
        mensagem = $" Livro {livro.Titulo} deletado com sucesso!"
    });
});

app.MapGet("api/livros", (string? tipoSelecionado, string? textoDigitado, BibliotecaDbContext dados) =>
{
    var consulta = dados.Livros.AsQueryable();

    if (string.IsNullOrEmpty(textoDigitado) || string.IsNullOrEmpty(tipoSelecionado))
    {
        return Results.Ok(consulta.ToList());
    }

    switch (tipoSelecionado.ToLower())
    {
        case "id":
            if (int.TryParse(textoDigitado, out int idValido))
            {
                consulta = consulta.Where(l => l.Id == idValido);
            }
            break;

        case "titulo":
            var palavrasTitulo = textoDigitado.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            consulta = consulta.Where(l => palavrasTitulo.Any(p => l.Titulo.ToLower().Contains(p, StringComparison.OrdinalIgnoreCase)));
            break;

        case "autor":
            var palavrasAutor = textoDigitado.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            consulta = consulta.Where(l => palavrasAutor.Any(p => l.Autor.ToLower().Contains(p, StringComparison.OrdinalIgnoreCase)));
            break;

        case "ano":
            if (int.TryParse(textoDigitado, out int anoValido))
            {
                consulta = consulta.Where(l => l.AnoLancamento == anoValido);
            }
            break;
    }

    return Results.Ok(consulta.ToList());
});

app.MapPut("api/livros/{id:int}", (int id, Livro livroAtualizado, BibliotecaDbContext dados) =>
{
    var livroExistente = dados.Livros.Find(id);

    if (livroExistente == null)
    {
        return Results.NotFound(new { mensagem = "Livro não encontrado para atualização!" });
    }

    livroExistente.Titulo = livroAtualizado.Titulo;
    livroExistente.Autor = livroAtualizado.Autor;
    livroExistente.AnoLancamento = livroAtualizado.AnoLancamento;

    dados.SaveChanges();

    return Results.Ok(livroExistente);
});

app.Urls.Add("http://localhost:5000"); // 👈 Obriga o C# a usar essa porta
app.Run();