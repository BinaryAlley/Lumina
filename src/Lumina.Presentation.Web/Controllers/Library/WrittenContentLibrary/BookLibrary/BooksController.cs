using Microsoft.AspNetCore.Mvc;
using System;

namespace Lumina.Presentation.Web.Controllers.Library.WrittenContentLibrary.BookLibrary;

[Route("/library/written-content-library/books-library/books/")]
public class BooksController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("/Views/Library/WrittenContentLibrary/BookLibrary/Books/Index.cshtml");
    }

    [HttpGet("{id}")]
    public IActionResult EditBook(Guid id)
    {
        return View("/Views/Library/WrittenContentLibrary/BookLibrary/Books/Item.cshtml");
    }
}
