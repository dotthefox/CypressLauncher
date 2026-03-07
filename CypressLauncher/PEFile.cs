using System.IO;
using System.Runtime.InteropServices;

namespace CypressLauncher;

public static class PEFile
{
	public struct IMAGE_DOS_HEADER
	{
		public ushort e_magic;

		public ushort e_cblp;

		public ushort e_cp;

		public ushort e_crlc;

		public ushort e_cparhdr;

		public ushort e_minalloc;

		public ushort e_maxalloc;

		public ushort e_ss;

		public ushort e_sp;

		public ushort e_csum;

		public ushort e_ip;

		public ushort e_cs;

		public ushort e_lfarlc;

		public ushort e_ovno;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public ushort[] e_res1;

		public ushort e_oemid;

		public ushort e_oeminfo;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
		public ushort[] e_res2;

		public int e_lfanew;
	}

	public struct IMAGE_FILE_HEADER
	{
		public ushort Machine;

		public ushort NumberOfSections;

		public uint TimeDateStamp;

		public uint PointerToSymbolTable;

		public uint NumberOfSymbols;

		public ushort SizeOfOptionalHeader;

		public ushort Characteristics;
	}

	public static IMAGE_FILE_HEADER GetNTHeaderFromPE(string filepath)
	{
		if (!File.Exists(filepath))
		{
			throw new FileNotFoundException(filepath);
		}
		using FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		using BinaryReader binaryReader = new BinaryReader(fileStream);
		IMAGE_DOS_HEADER iMAGE_DOS_HEADER = FromBinaryReader<IMAGE_DOS_HEADER>(binaryReader);
		if (iMAGE_DOS_HEADER.e_magic != 23117)
		{
			throw new InvalidDataException("Invalid DOS Header");
		}
		fileStream.Seek(iMAGE_DOS_HEADER.e_lfanew, SeekOrigin.Begin);
		if (binaryReader.ReadUInt32() != 17744)
		{
			throw new InvalidDataException("Invalid NT Header");
		}
		return FromBinaryReader<IMAGE_FILE_HEADER>(binaryReader);
	}

	private static T FromBinaryReader<T>(BinaryReader reader)
	{
		GCHandle gCHandle = GCHandle.Alloc(reader.ReadBytes(Marshal.SizeOf(typeof(T))), GCHandleType.Pinned);
		T result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
		gCHandle.Free();
		return result;
	}
}
