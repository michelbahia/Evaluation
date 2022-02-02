#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/TodoItems")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            return await _context.TodoItems
                .Select(x => ItemToDTO(x))
                .ToListAsync();
        }

        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(CardRequest cardRequest)
        {
            var cardReq = new CardRequest
            {
                CustomerId = cardRequest.CustomerId,
                CardNumber = cardRequest.CardNumber,
                CVV = cardRequest.CVV,
            };

            var todoItem = new TodoItem
            {
                CustomerId = cardReq.CustomerId,
                CardNumber = cardReq.CardNumber,
                CVV = cardReq.CVV,

                CardId = cardRequest.CustomerId + 2,
                RegistrationDate = DateTime.Now,
                Token = CreateToken(cardRequest.CardNumber, cardRequest.CVV),
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTodoItems),
                new { id = todoItem.CustomerId },
                ItemToDTOReturn(todoItem));
        }

        private long CreateToken(long cardNumber, int CVV)
        {
            string cardString = cardNumber.ToString();
            string token = cardString.Substring(cardString.Length - 4, 4);

            int[] list = new int[4];
            int cont = 0;
            foreach (var item in token)
            {
                list[cont] = (Int32.Parse(item.ToString()));
                cont++;
            }

            int countCvv = CVV.ToString().Count();
            int [] arryAux = new int[4];
            while (countCvv > 0)
            {
                arryAux = rotation(list);
                countCvv--;
            }

            string strToken = string.Empty;
            foreach (var item in arryAux)
            {
                strToken += item.ToString();
            }

            long outToken = long.Parse(strToken);
            return outToken;
        }

        private int[] rotation(int[] list)
        {
            int n = list.Length;
            int aux = list[n - 1];

            for (int i = n-1; i > 0; i--)
            {
                list[i] = list[i - 1];
            }
            list[0] = aux;

            return list;
        }

        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                RegistrationDate = todoItem.RegistrationDate,
                Token = todoItem.Token,
                CardId = todoItem.CardId,

                CustomerId = todoItem.CustomerId,
                CardNumber = todoItem.CardNumber,
                CVV = todoItem.CVV,
                Id = todoItem.Id
            };

        private static TodoItemDTOReturn ItemToDTOReturn(TodoItem todoItem) =>
            new TodoItemDTOReturn
            {
                RegistrationDate = todoItem.RegistrationDate,
                Token = todoItem.Token,
                CardId = todoItem.CardId
            };

        [HttpGet("tokenValidation")]
        public async Task<bool> TokenValidation(int customerId, long cardId, long token, int cvv)
        {
            var item = await _context.TodoItems.FirstAsync(x => x.CardId == cardId && x.CustomerId == customerId);
            if (item == null) return false;

            if (!ValidationTimeToken(item.RegistrationDate)) return false;

            Console.WriteLine(item.CardNumber);

            var generateToken = CreateToken(item.CardNumber, cvv);
            if (generateToken != token) return false;

            return true;
        }

        private bool ValidationTimeToken(DateTime registrationDate)
        {
            var dt1 = DateTime.Now;
            var dt2 = registrationDate;

            TimeSpan ts = dt1 - dt2;

            if (ts.TotalMinutes >= 30) return false;

            return true;
        }
    }
}