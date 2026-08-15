using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using HSO_WebAPI.Models;

public class APIController : ControllerBase
{
    private HSOEntities db = new HSOEntities();

    public APIController()
    {
    }

    #region Load basic data
    [HttpGet]
    [Route("api/load/Map/full")]
    public IActionResult LoadMapFull()
    {
        var mapList = db.Maps.ToList();
        var mapMobs = db.MapMobs.ToList();
        var mapNpcs = db.MapNpcs.ToList();

        var mapData = mapList.Select(map => new
        {
            map = new
            {
                map.Idmap,
                map.NameMap,
            },

            mobsData =
                (from mm in db.MapMobs
                 join mob in db.Mobs on mm.Idmob equals mob.Idmob
                 where mm.Idmap == map.Idmap
                 select new
                 {
                     mob = new
                     {
                         mob.Idmob,
                         mob.NameMob,
                         mob.Boss,
                         mob.Level,
                         mob.Hp
                     },

                     id = mm.Id,
                     posX = mm.PosX,
                     posY = mm.PosY,
                 }).ToList(),

            npcsData =
                (from mn in db.MapNpcs
                 join npc in db.Npcs on mn.Idnpc equals npc.Idnpc
                 where mn.Idmap == map.Idmap
                 select new
                 {
                     npc = new
                     {
                         npc.Idnpc,
                         npc.NameNpc,
                     },

                     posX = mn.PosX,
                     posY = mn.PosY
                 }).ToList()
        }).ToList();

        return Ok(mapData);
    }

    [HttpGet]
    [Route("api/load/Item0/full")]
    public IActionResult LoadItem0Full()
    {
        var item0List = db.Item0s.ToList();
        var itemAttrs = db.Item0Attributes.ToList();
        var attributes = db.Attributes.ToList();

        var item0Data = item0List.Select(item => new
        {
            item0 = new
            {
                item.Iditem0,
                item.NameItem0,
                item.TypeItem0,
                item.Idschool
            },
            item0_Attributes = itemAttrs
                .Where(a => a.Iditem0 == item.Iditem0 && a.Category == 1)
                .Select(a => new
                {
                    a.Idattribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.Idattribute equals attr.Idattribute
                where a.Iditem0 == item.Iditem0
                select new
                {
                    attr.Idattribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(item0Data);
    }

    [HttpGet]
    [Route("api/load/Item1/full")]
    public IActionResult LoadItem1Full()
    {
        var item1List = db.Item1s.ToList();
        var itemAttrs = db.Item1Attributes.ToList();
        var attributes = db.Attributes.ToList();

        var item1Data = item1List.Select(item => new
        {
            item1 = new
            {
                item.Iditem1,
                item.NameItem1,
                item.TypeItem1,
            },
            item1_Attributes = itemAttrs
                .Where(a => a.Iditem1 == item.Iditem1 && a.Category == 1)
                .Select(a => new
                {
                    a.Idattribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.Idattribute equals attr.Idattribute
                where a.Iditem1 == item.Iditem1
                select new
                {
                    attr.Idattribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(item1Data);
    }

    [HttpGet]
    [Route("api/load/Item2/full")]
    public IActionResult LoadItem2Full()
    {
        var item2List = db.Item2s.ToList();
        if (item2List == null || !item2List.Any())
            return NotFound();
        return Ok(item2List);
    }

    [HttpGet]
    [Route("api/load/Item3/full")]
    public IActionResult LoadItem3Full()
    {
        var item3List = db.Item3s.ToList();
        if (item3List == null || !item3List.Any())
            return NotFound();
        return Ok(item3List);
    }

    [HttpGet]
    [Route("api/load/Item4/full")]
    public IActionResult LoadItem4Full()
    {
        var item4List = db.Item4s.ToList();
        if (item4List == null || !item4List.Any())
            return NotFound();

        return Ok(item4List);
    }
    #endregion

    [HttpGet]
    [Route("api/account/login")]
    public IActionResult Login(string username, string password)
    {
        var account = db.Accounts.FirstOrDefault(a => a.Username == username && a.Password == password);

        if (account == null)
            return NotFound();

        return Ok(account);
    }

    [HttpPost]
    [Route("api/account/register")]
    public IActionResult Register([FromBody] RegisterRequest request)
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

        // Tạo inventory khởi đầu và thêm vào bảng lúc register
        var newInventory = new AccountItem0
        {
            Idaccount = newAccount.Idaccount,  // gán IDAccount vừa tạo
            Iditem0 = 1, // Giả sử item khởi đầu có IDItem0 là 1
            Category = 1  // Giả sử category khởi đầu là 0
        };
        db.AccountItem0s.Add(newInventory);
        
        // Gán IDAccount cho equipment và thêm vào bảng
        foreach (var eq in request.Equipment)
        {
            eq.Idaccount = newAccount.Idaccount;
            db.AccountEquipments.Add(eq);
        }

        db.SaveChanges();

        return Ok(new
        {
            message = "Đăng ký thành công!",
            IDAccount = newAccount.Idaccount
        });
    }

    #region Load equipment data
    [HttpGet]
    [Route("api/account/{idAccount}/equipment")]
    public IActionResult Equipment(int idAccount)
    {
        var equipments = db.AccountEquipments.Where(x => x.Idaccount == idAccount).ToList();

        if (!equipments.Any())
            return NotFound();

        var itemAttrs = db.Item0Attributes.ToList();
        var attributes = db.Attributes.ToList();

        var result = equipments.Select(eq => new
        {
            id = eq.Id,
            idItem0_1 = eq.Iditem01,
            nameItem0_1 = db.Item0s.Where(i => i.Iditem0 == eq.Iditem01).Select(i => i.NameItem0).FirstOrDefault(),
            category = eq.Category,
            slotName = eq.SlotName,

            item0_Attributes = itemAttrs
                .Where(a =>
                    a.Iditem0 == eq.Iditem01 &&
                    a.Category == eq.Category)
                .Select(a => new
                {
                    a.Idattribute,
                    a.Value,
                    a.Category
                })
                .ToList(),

            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.Idattribute equals attr.Idattribute
                where a.Iditem0 == eq.Iditem01
                select new
                {
                    attr.Idattribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(result);
    }
    #endregion

    #region Load inventory data
    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem0")]
    public IActionResult InventoryItem0(int idAccount)
    {
        var inventory = db.AccountItem0s.Where(x => x.Idaccount == idAccount).ToList();

        if (!inventory.Any())
            return NotFound();

        var item0List = db.Item0s.ToList();
        var itemAttrs = db.Item0Attributes.ToList();
        var attributes = db.Attributes.ToList();

        var inventoryData = inventory.Select(inv => new
        {
            id = inv.Id,
            idItem0 = inv.Iditem0,
            nameItem0 = item0List.First(i => i.Iditem0 == inv.Iditem0).NameItem0,
            typeItem0 = item0List.First(i => i.Iditem0 == inv.Iditem0).TypeItem0,
            category = item0List.First(i => i.Iditem0 == inv.Iditem0).Level,
            idschool = item0List.First(i => i.Iditem0 == inv.Iditem0).Idschool,

            item0_Attributes = itemAttrs
                .Where(a => a.Iditem0 == inv.Iditem0 && a.Category == inv.Category)
                .Select(a => new
                {
                    a.Idattribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.Idattribute equals attr.Idattribute
                where a.Iditem0 == inv.Iditem0
                select new
                {
                    attr.Idattribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(inventoryData);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem1")]
    public IActionResult InventoryItem1(int idAccount)
    {
        var inventory = db.AccountItem1s.Where(x => x.Idaccount == idAccount).ToList();

        if (!inventory.Any())
            return NotFound();

        var item1List = db.Item1s.ToList();
        var itemAttrs = db.Item1Attributes.ToList();
        var attributes = db.Attributes.ToList();

        var inventoryData = inventory.Select(inv => new
        {
            idItem1 = inv.Iditem1,
            nameItem1 = item1List.First(i => i.Iditem1 == inv.Iditem1).NameItem1,
            typeItem1 = item1List.First(i => i.Iditem1 == inv.Iditem1).TypeItem1,

            item1_Attributes = itemAttrs
                .Where(a => a.Iditem1 == inv.Iditem1 && a.Category == 1)
                .Select(a => new
                {
                    a.Idattribute,
                    a.Value,
                    a.Category
                })
                .ToList(),
            nameAttributes = (
                from a in itemAttrs
                join attr in attributes on a.Idattribute equals attr.Idattribute
                where a.Iditem1 == inv.Iditem1
                select new
                {
                    attr.Idattribute,
                    attr.NameAttribute
                }
            ).Distinct().ToList()
        }).ToList();

        return Ok(inventoryData);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem2")]
    public IActionResult InventoryItem2(int idAccount)
    {
        var item2 = db.AccountItem2s.Where(x => x.Idaccount == idAccount).ToList();

        if (!item2.Any())
            return NotFound();

        return Ok(item2);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem3")]
    public IActionResult InventoryItem3(int idAccount)
    {
        var item3 = db.AccountItem3s.Where(x => x.Idaccount == idAccount).ToList();

        if (!item3.Any())
            return NotFound();

        return Ok(item3);
    }

    [HttpGet]
    [Route("api/account/{idAccount}/inventoryItem4")]
    public IActionResult InventoryItem4(int idAccount)
    {
        var item4 = db.AccountItem4s.Where(x => x.Idaccount == idAccount).ToList();

        if (!item4.Any())
            return NotFound();

        return Ok(item4);
    }
    #endregion

    #region Hoán đổi Item0 giữa inventory và equipment
    [HttpPost]
    [Route("api/account/{idAccount}/equipItem0/{id}")]
    public IActionResult EquipItem0(int idAccount, int id, string slotName)
    {
        var inventoryData = db.AccountItem0s.Where(x => x.Idaccount == idAccount && x.Id == id).FirstOrDefault();

        var typeInventoryData = db.Item0s.Where(x => x.Iditem0 == inventoryData.Iditem0)
            .Select(x => new
            {
                x.TypeItem0,
                x.Idschool
            })
            .FirstOrDefault();

        string typeCheck = typeInventoryData.TypeItem0;

        if (typeCheck.Equals("Ring"))
        {
            typeCheck = slotName;
        }

        var idSchool = db.Accounts.Where(x => x.Idaccount == idAccount).Select(x => x.Idschool).FirstOrDefault();
        if (idSchool != typeInventoryData.Idschool && typeInventoryData.Idschool != 0)
        {
            return BadRequest("Không thể trang bị vật phẩm từ trường phái khác.");
        }

        var equipmentData = db.AccountEquipments.Where(x => x.Idaccount == idAccount && x.SlotName == typeCheck).FirstOrDefault();

        int tempItemId = equipmentData.Iditem01;
        int tempCategory = equipmentData.Category;

        equipmentData.Iditem01 = inventoryData.Iditem0;
        equipmentData.Category = inventoryData.Category;

        if (tempItemId == 0)
        {
            db.AccountItem0s.Remove(inventoryData);
        }
        else
        {
            inventoryData.Iditem0 = tempItemId;
            inventoryData.Category = tempCategory;
        }

        db.SaveChanges();

        return Ok("Equipped");
    }
    #endregion
}
public class RegisterRequest
{
    public Account Account { get; set; }
    public List<AccountEquipment> Equipment { get; set; }
}