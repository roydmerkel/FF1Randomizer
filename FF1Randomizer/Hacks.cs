﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomUtilities;

namespace FF1Randomizer
{
	public partial class FF1Rom : NesRom
	{
		public const int Nop = 0xEA;
		public const int SardaOffset = 0x393E9;
		public const int SardaSize = 7;
		public const int CanoeSageOffset = 0x39482;
		public const int CanoeSageSize = 5;
		public const int PartyShuffleOffset = 0x312E0;
		public const int PartyShuffleSize = 3;

		public void EnableEarlyRod()
		{
			var nops = new byte[SardaSize];
			for (int i = 0; i < nops.Length; i++)
			{
				nops[i] = Nop;
			}

			Put(SardaOffset, nops);
		}

		public void EnableEarlyCanoe()
		{
			var nops = new byte[CanoeSageSize];
			for (int i = 0; i < nops.Length; i++)
			{
				nops[i] = Nop;
			}

			Put(CanoeSageOffset, nops);
		}

		public void DisablePartyShuffle()
		{
			var nops = new byte[PartyShuffleSize];
			for (int i = 0; i < nops.Length; i++)
			{
				nops[i] = Nop;
			}
			Put(PartyShuffleOffset, nops);

			nops = new byte[2];
			for (int i = 0; i < nops.Length; i++)
			{
				nops[i] = Nop;
			}
			Put(0x39A6B, nops);
			Put(0x39A74, nops);
		}

		public void EnableSpeedHacks()
		{
			// Screen wipe
			Data[0x3D6EE] = 0x08; // These two values must evenly divide 224 (0xE0), default are 2 and 4
			Data[0x3D6F5] = 0x10;
			Data[0x3D713] = 0x0A; // These two values must evenly divide 220 (0xDC), default are 2 and 4
			Data[0x3D71A] = 0x14; // Don't ask me why they aren't the same, it's the number of scanlines to stop the loop at

			// Dialogue boxes
			Data[0x3D620] = 0x0B; // These two values must evenly divide 88 (0x58), the size of the dialogue box
			Data[0x3D699] = 0x0B;

			// Battle entry
			Data[0x3D90A] = 0x11; // This can be just about anything, default is 0x41, sfx lasts for 0x20

			// All kinds of palette cycling
			Data[0x3D955] = 0x00; // This value is ANDed with a counter, 0x03 is default, 0x01 is double speed, 0x00 is quadruple

			// Battle
			Data[0x31ECE] = 0x60; // Double character animation speed
			Data[0x2DFB4] = 0x04; // Quadruple run speed

			Data[0x33D4B] = 0x04; // Explosion effect count (big enemies), default 6
			Data[0x33CCD] = 0x04; // Explosion effect count (small enemies), default 8
			Data[0x33DAA] = 0x04; // Explosion effect count (mixed enemies), default 15
		}

		public void EnableIdentifyTreasures()
		{
			Put(0x2B192, Blob.FromHex("C1010200000000"));
		}

		public void EnableDash()
		{
			Put(0x03D077, Blob.FromHex("4A252DD002A54224205002A9044A6900853460"));
		}

		public void EnableBuyTen()
		{
			Put(0x380FF, Blob.FromHex("100001110001120001130000"));
			Put(0x38248, Blob.FromHex("8BB8BC018BB8BCFF8180018EBB5B00"));
			Put(0x3A8E4, Blob.FromHex("A903203BAAA9122026AAA90485634C07A9A903203BAAA91F2026AAA90385634C07A9EA"));
			Put(0x3A32C, Blob.FromHex("71A471A4"));
			Put(0x3A45A, Blob.FromHex("2066A420EADD20EFA74CB9A3A202BD0D039510CA10F860A909D002A925205BAAA5664A900920B1A8B0EC0662900520F5A8B0E3A562F0054A90DCA909856AA90D205BAA2057A8B0D82076AAA66AF017861318A200A003BD0D0375109D0D03E888D0F4C613D0EB2068AA20C2A8B0ADA562D0A9208CAA9005A9104C77A4AE0C03BD206038656AC9649005A90C4C77A49D206020F3A4A9134C77A4A200A00338BD1C60FD0D039D1C60E888D0F34CEFA7"));
			Put(0x3AA65, Blob.FromHex("2076AAA90E205BAA2066A4208E8E4C32AAA662BD00038D0C0320B9ECA202B5109D0D03CA10F860A202BD0D03DD1C60D004CA10F51860"));
			Put(0x3A390, Blob.FromHex("208CAA"));
			Put(0x3A3E0, Blob.FromHex("208CAA"));
			Put(0x3A39D, Blob.FromHex("20F3A4"));
			Put(0x3A404, Blob.FromHex("20F3A4"));
			Put(0x3AACB, Blob.FromHex("18A202B5106A95109D0D03CA10F5"));
		}

		public void EasterEggs()
		{
			Put(0x2ADDE, Blob.FromHex("91251A682CC18EB1B74DB32505BE9296991E2F1AB6A4A9A8BE05C1C1C1C19B929900"));
		}
	}
}
