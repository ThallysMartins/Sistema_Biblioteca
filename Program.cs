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

app.MapGet("/api/livros", (BibliotecaDbContext banco) =>
{
    // Busca a lista de livros que está no banco e responde com Sucesso (Status 200)
    return Results.Ok(banco.Livros.ToList());
});

app.MapPost("/api/livros", (Livro novoLivro, BibliotecaDbContext banco) =>
{
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
            var palavrasTitulo = textoDigitado.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            consulta = consulta.Where(l => palavrasTitulo.Any(p => l.Titulo.Contains(p, StringComparison.OrdinalIgnoreCase)));
            break;

        case "autor":
            var palavrasAutor = textoDigitado.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            consulta = consulta.Where(l => palavrasAutor.Any(p => l.Autor.Contains(p, StringComparison.OrdinalIgnoreCase)));
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

app.Urls.Add("http://localhost:5000"); // 👈 Obriga o C# a usar essa porta
app.Run();