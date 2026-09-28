using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CircleAnimation
{
    public class DbHelper
    {
        private readonly string _connectionString = "Host=localhost; Database=animation_db; Username=postgres; Password=Vavevenno77732!";

        /// <summary>
        /// Сохраняет время запуска анимации в БД.
        /// </summary>
        public async Task SaveStartAsync(AnimationRecord record)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = "INSERT INTO animation_starts (started_at) VALUES (@started_at)";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("started_at", record.StartedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Загружает всю историю запусков из БД (свежие — сверху).
        /// </summary>
        public async Task<List<AnimationRecord>> LoadRecordsAsync()
        {
            var records = new List<AnimationRecord>();

            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"SELECT id, started_at
                        FROM animation_starts
                        ORDER BY started_at DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                records.Add(new AnimationRecord
                {
                    Id = reader.GetInt32(0),
                    StartedAt = reader.GetDateTime(1)
                });
            }
            return records;
        }
    }
}