using System;

namespace QuanLyDaoTaoNoiBo
{
    // Exception thrown when a result has been locked and the operation is not allowed.
    public class KetQuaDaKhoaException : Exception
    {
        public KetQuaDaKhoaException() { }
        public KetQuaDaKhoaException(string message) : base(message) { }
        public KetQuaDaKhoaException(string message, Exception inner) : base(message, inner) { }
    }
}
