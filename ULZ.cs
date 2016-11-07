using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AC3_ULZ_decomp
{
    class ULZ
    {
        static public MemoryStream Uncompress(BinaryReader ulz)
        {
            char[] id = ulz.ReadChars(4);
            int u_size = ulz.ReadInt32();
            uint u_pos = ulz.ReadUInt32();

            int type = u_size >> 24;

            switch (type)
            {
                case 0x00:
                    return Uncompress_v0(ulz, u_size, u_pos);
                case 0x02:
                    return Uncompess_v2(ulz, u_size, u_pos);
                default:
                    throw new Exception("Uknown ULZ compression!");
            }
        }

        static public MemoryStream Uncompess_v2(BinaryReader ulz, int u_size, uint u_pos)
        {
            {
                MemoryStream buffer = new MemoryStream();

                try
                {
                    ushort unk2 = (ushort)((u_pos & 0xFF000000) >> 24);
                    ushort unk3 = (ushort)((1 << (int)unk2) + 0xFFFF);

                    u_size &= 0x00FFFFFF;
                    u_pos &= 0x00FFFFFF;

                    uint c_pos = ulz.ReadUInt32();

                    long f_pos = ulz.BaseStream.Position;

                    int flags = ulz.ReadInt32();
                    f_pos += 4;

                    int flag_pos = 0x20;

                    while (true)
                    {
                        flag_pos -= 1;

                        if (flag_pos < 0)
                        {
                            ulz.BaseStream.Seek(f_pos, SeekOrigin.Begin);
                            flags = ulz.ReadInt32();
                            f_pos += 4;
                            flag_pos = 0x1F;
                        }

                        bool is_comp = (0 <= flags);
                        flags <<= 1;

                        if (is_comp)
                        {
                            ulz.BaseStream.Seek(c_pos, SeekOrigin.Begin);
                            ushort c = ulz.ReadUInt16();
                            c_pos += 2;

                            uint pos = (uint)(c & unk3);
                            pos += 1;

                            uint run = (uint)(c >> unk2);
                            run += 3;
                            u_size -= (int)run;

                            byte[] temp = buffer.GetBuffer();
                            long t_pos = buffer.Position;

                            for (int j = 0; j < run; j++)
                            {
                                buffer.WriteByte(temp[(t_pos - pos) + j]);
                            }
                        }
                        else
                        {
                            ulz.BaseStream.Seek(u_pos, SeekOrigin.Begin);
                            byte u = ulz.ReadByte();
                            buffer.WriteByte(u);
                            u_pos++;

                            u_size--;
                        }

                        if (u_size <= 0)
                            break;
                    }
                }
                catch (Exception e)
                {
                    ulz.Close();
                    return null;
                }

                ulz.Close();
                return buffer;
            }
        }

        static public MemoryStream Uncompress_v0(BinaryReader ulz, int u_size, uint u_pos)
        {
            MemoryStream buffer = new MemoryStream();

            try
            {
                ushort unk2 = (ushort)((u_pos & 0xFF000000) >> 24);
                ushort unk3 = (ushort)((1 << (int)unk2) + 0xFFFF);

                u_size &= 0x00FFFFFF;
                u_pos &= 0x00FFFFFF;

                uint c_pos = ulz.ReadUInt32();

                long f_pos = ulz.BaseStream.Position;

                int flags = ulz.ReadInt32();
                f_pos += 4;

                bool is_comp = (0 < flags);

                while (true)
                {
                    flags <<= 1;
                    if (flags == 0)
                    {
                        ulz.BaseStream.Seek(f_pos, SeekOrigin.Begin);
                        flags = ulz.ReadInt32();

                        if (flags == 0)
                        {
                            break;
                        }

                        f_pos += 4;
                        is_comp = (0 < flags);
                        flags <<= 1;
                    }

                    // Check to see if u_pos == 0?
                    // TODO : Figure why I put this here... As I can't remember...

                    if (u_pos == 0) // Need to temp this somewhere...
                    {

                    }

                    if (is_comp)
                    {
                        ulz.BaseStream.Seek(c_pos, SeekOrigin.Begin);
                        ushort c = ulz.ReadUInt16();
                        c_pos += 2;

                        uint pos = (uint)(c & unk3);
                        pos += 1;

                        uint run = (uint)(c >> unk2);
                        run += 3;

                        byte[] temp = buffer.GetBuffer();
                        long t_pos = buffer.Position;

                        for (int j = 0; j < run; j++)
                        {
                            buffer.WriteByte(temp[(t_pos - pos) + j]);
                        }
                    }
                    else
                    {
                        ulz.BaseStream.Seek(u_pos, SeekOrigin.Begin);
                        byte u = ulz.ReadByte();
                        buffer.WriteByte(u);
                        u_pos++;
                    }

                    is_comp = (0 < flags);

                }
            }
            catch (Exception e)
            {
                ulz.Close();
                return null;
            }

            ulz.Close();
            return buffer;
        }
    }
}
