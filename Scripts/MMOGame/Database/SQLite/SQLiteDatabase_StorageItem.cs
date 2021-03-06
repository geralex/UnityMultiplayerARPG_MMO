﻿using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        private void CreateStorageItem(int idx, StorageType storageType, string storageOwnerId, CharacterItem characterItem)
        {
            ExecuteNonQuery("INSERT INTO storageitem (id, idx, storageType, storageOwnerId, dataId, level, amount, durability, exp, lockRemainsDuration, ammo) VALUES (@id, @idx, @storageType, @storageOwnerId, @dataId, @level, @amount, @durability, @exp, @lockRemainsDuration, @ammo)",
                new SqliteParameter("@id", new StorageItemId(storageType, storageOwnerId, idx).GetId()),
                new SqliteParameter("@idx", idx),
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId),
                new SqliteParameter("@dataId", characterItem.dataId),
                new SqliteParameter("@level", characterItem.level),
                new SqliteParameter("@amount", characterItem.amount),
                new SqliteParameter("@durability", characterItem.durability),
                new SqliteParameter("@exp", characterItem.exp),
                new SqliteParameter("@lockRemainsDuration", characterItem.lockRemainsDuration),
                new SqliteParameter("@ammo", characterItem.ammo));
        }

        private bool ReadStorageItem(SQLiteRowsReader reader, out CharacterItem result, bool resetReader = true)
        {
            if (resetReader)
                reader.ResetReader();

            if (reader.Read())
            {
                result = new CharacterItem();
                result.dataId = reader.GetInt32("dataId");
                result.level = (short)reader.GetInt32("level");
                result.amount = (short)reader.GetInt32("amount");
                result.durability = reader.GetFloat("durability");
                result.exp = reader.GetInt32("exp");
                result.lockRemainsDuration = reader.GetFloat("lockRemainsDuration");
                result.ammo = reader.GetInt32("ammo");
                return true;
            }
            result = CharacterItem.Empty;
            return false;
        }

        public override List<CharacterItem> ReadStorageItems(StorageType storageType, string storageOwnerId)
        {
            List<CharacterItem> result = new List<CharacterItem>();
            SQLiteRowsReader reader = ExecuteReader("SELECT * FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId ORDER BY idx ASC",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
            CharacterItem tempInventory;
            while (ReadStorageItem(reader, out tempInventory, false))
            {
                result.Add(tempInventory);
            }
            return result;
        }

        public override void UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> characterItems)
        {
            BeginTransaction();
            DeleteStorageItems(storageType, storageOwnerId);
            for (int i = 0; i < characterItems.Count; ++i)
            {
                CreateStorageItem(i, storageType, storageOwnerId, characterItems[i]);
            }
            EndTransaction();
        }

        public void DeleteStorageItems(StorageType storageType, string storageOwnerId)
        {
            ExecuteNonQuery("DELETE FROM storageitem WHERE storageType=@storageType AND storageOwnerId=@storageOwnerId",
                new SqliteParameter("@storageType", (byte)storageType),
                new SqliteParameter("@storageOwnerId", storageOwnerId));
        }
    }
}
