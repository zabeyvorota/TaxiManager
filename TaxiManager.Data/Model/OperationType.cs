namespace TaxiManager.Data.Model
{
    /// <summary>
    /// ������������ �������� � �������
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// ��������� ������
        /// </summary>
        Select,

        /// <summary>
        /// ���������� � ����������
        /// </summary>
        AddOrUpdate,

        /// <summary>
        /// ��������
        /// </summary>
        Delete,
        /// <summary>
        /// ����������� ��������� �����
        /// </summary>
        Admin
    }
}