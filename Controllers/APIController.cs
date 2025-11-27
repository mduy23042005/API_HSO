using System.Linq;
using System.Web.Http;

public class APIController : ApiController
{
    private HSOEntities.Models.HSOEntities db = new HSOEntities.Models.HSOEntities();
    public APIController()
    {
        db.Configuration.ProxyCreationEnabled = false; // không tạo proxy
        db.Configuration.LazyLoadingEnabled = false;  // không lazy load
    }

    [HttpGet]
    [Route("api/account/login")]
    public IHttpActionResult Login(string username, string password)
    {
        var account = db.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    [HttpPost]
    [Route("api/account/register")]
    public IHttpActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null || request.Account == null || request.Equipment == null)
            return BadRequest("Dữ liệu gửi lên không hợp lệ.");

        // Kiểm tra username hoặc NameChar đã tồn tại
        if (db.Accounts.Any(a => a.Username == request.Account.Username))
            return BadRequest("{\"errorField\":\"Username\",\"message\":\"Username đã tồn tại.\"}");

        if (db.Accounts.Any(a => a.NameChar == request.Account.NameChar))
            return BadRequest("{\"errorField\":\"NameChar\",\"message\":\"Tên nhân vật đã tồn tại.\"}");

        // Tạo account mới
        var newAccount = request.Account;
        db.Accounts.Add(newAccount);

        // Gán IDAccount cho equipment và thêm vào bảng
        foreach (var eq in request.Equipment)
        {
            eq.IDAccount = newAccount.IDAccount; // gán IDAccount vừa tạo
            db.Account_Equipment.Add(eq);
        }

        db.SaveChanges();

        return Ok(new
        {
            message = "Đăng ký thành công!",
            IDAccount = newAccount.IDAccount
        });
    }

    [HttpGet]
    [Route("api/account/{idAccount}/getHair")]
    public IHttpActionResult GetHair(int idAccount)
    {
        var hair = db.Accounts.Where(x => x.IDAccount == idAccount).Select(x => x.Hair).FirstOrDefault();
        return Ok(hair);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/equipment")]
    public IHttpActionResult Equipment(int idAccount)
    {
        var item0_1 = db.Account_Equipment
            .Where(x => x.IDAccount == idAccount)
            .Select(x => new
            {
                IDAccount = x.IDAccount,
                IDItem0_1 = x.IDItem0_1,
                Category = x.Category
            })
            .ToList();

        if (!item0_1.Any())
            return NotFound();

        return Ok(item0_1);
    }

    #region Đọc Attribute từ equipment
    [HttpGet]
    [Route("api/account/{idAccount}/equipItem/{idItem0}/listAttributes")]
    public IHttpActionResult EquipmentListAttribute(int idItem)
    {
        /*
        var id = db.Account_Equipment.Where(x => x.IDItem0_1 == idItem).Select(x => x.ID).FirstOrDefault();

        //Tạm thời sẽ lấy Attribute từ bảng Item0_Attribute
        var listAttributes = db.Account_Equipment_Attribute.Where(x => x.IDAccountEquipment == id).ToList();
        */

        var category = db.Account_Equipment.Where(x => x.IDItem0_1 == idItem).Select(x => x.Category).FirstOrDefault();

        var listAttributes = db.Item0_Attribute.Where(x => x.IDItem0 == idItem && x.Category == category).ToList();

        if (!listAttributes.Any())
            return NotFound();

        return Ok(listAttributes);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/equipItem/{idItem0}/listAttributes/{idAttribute}")]
    public IHttpActionResult EquipmentNameAttribute(int idAttribute)
    {
        var nameAttributes = db.Attributes.Where(x => x.IDAttribute == idAttribute).Select(x => x.NameAttribute).FirstOrDefault();

        if (!nameAttributes.Any())
            return NotFound();

        return Ok(nameAttributes);
    }
    #endregion

    [HttpGet]
    [Route("api/account/{idAccount}/inventory")]
    public IHttpActionResult Inventory(int idAccount)
    {
        var item0 = db.Account_Item0.Where(x => x.IDAccount == idAccount)
            .Select(i => new { i.IDItem0, i.Category }).ToList();

        if (item0 == null)
            return NotFound();

        return Ok(item0);
    }

    #region Đọc Attribute từ inventory
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem/{idItem0}/listAttributes")]
    public IHttpActionResult InventoryListAttribute(int idItem)
    {
        var category = db.Account_Item0.Where(x => x.IDItem0 == idItem).Select(x => x.Category).FirstOrDefault();

        //Tạm thời sẽ lấy Attribute từ bảng Item0_Attribute
        var listAttributes = db.Item0_Attribute.Where(x => x.IDItem0 == idItem && x.Category == category).ToList();

        if (!listAttributes.Any())
            return NotFound();

        return Ok(listAttributes);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem/{idItem0}/listAttributes/{idAttribute}")]
    public IHttpActionResult InventoryNameAttribute(int idAttribute)
    {
        var nameAttributes = db.Attributes.Where(x => x.IDAttribute == idAttribute).Select(x => x.NameAttribute).FirstOrDefault();

        if (!nameAttributes.Any())
            return NotFound();

        return Ok(nameAttributes);
    }
    #endregion

    #region Hoán đổi Item0 giữa inventory và equipment
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem0/{idItem0}")]
    public IHttpActionResult School(int idAccount)
    {
        var idSchool = db.Accounts.Where(x => x.IDAccount == idAccount).FirstOrDefault();
        if (idSchool == null)
            return NotFound();
        return Ok(idSchool);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem0/{idItem0}")]
    public IHttpActionResult InventoryItem(int idAccount, int idItem0)
    {
        var item0 = db.Account_Item0.Where(x => x.IDAccount == idAccount && x.IDItem0 == idItem0).FirstOrDefault();

        if (item0 == null)
            return NotFound();

        return Ok(item0);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem0/{idItem0}/typeItem0")]
    public IHttpActionResult InventoryTypeItem(int idItem0)
    {
        var typeItem0 = db.Item0.Where(x => x.IDItem0 == idItem0).Select(x => new { x.TypeItem0, x.IDSchool }).FirstOrDefault();

        if (typeItem0 == null)
            return NotFound();

        return Ok(typeItem0);
    }
    [HttpGet]
    [Route("api/account/{idAccount}/equipItem/{IDItem0}/slotData")]
    public IHttpActionResult EquipmentSlotData(int idAccount, string slotName)
    {
        var slotData = db.Account_Equipment.Where(x => x.IDAccount == idAccount && x.SlotName == slotName)
            .Select(x => new { x.ID, x.IDAccount, x.IDItem0_1, x.SlotName, x.Category }).FirstOrDefault();

        return Ok(slotData);
    }

    [HttpPost]
    [Route("api/account/equipment/equip")]
    public IHttpActionResult EquipItem(int idAccount, string slotName, int idItem0, int category)
    {
        var equipmentSlot = db.Account_Equipment.Where(x => x.IDAccount == idAccount && x.SlotName == slotName).FirstOrDefault();

        if (equipmentSlot == null)
            return NotFound();

        equipmentSlot.IDItem0_1 = idItem0;   // gán item mới vào slot
        equipmentSlot.Category = category;

        db.SaveChanges();

        return Ok("Equipped!");
    }
    [HttpPost]
    [Route("api/account/inventory/return")]
    public IHttpActionResult ReturnItemToInventory(int idAccount, int idItem0, int category, int inventorySlot)
    {
        var inventoryItem = db.Account_Item0.Where(x => x.IDAccount == idAccount).ToList();

        if (inventoryItem == null)
            return NotFound();

        if (idItem0 == 0)
        {
            db.Account_Item0.Remove(inventoryItem[inventorySlot]);
            db.SaveChanges();

            return Ok("Equipped to empty slot. Item removed from inventory.");
        }

        inventoryItem[inventorySlot].IDItem0 = idItem0;
        inventoryItem[inventorySlot].Category = category;

        db.SaveChanges();

        return Ok("Returned to inventory!");
    }
    #endregion
}
