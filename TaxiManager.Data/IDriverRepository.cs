using System;
using System.Collections.Generic;

using TaxiManager.Data.Model;

namespace TaxiManager.Data
{
    /// <summary>
    /// ��������� ��������� ����������� ������� � �������� ��������
    /// </summary>
    public interface IDriverRepository
    {
        /// <summary>
        /// ����� ��������� ������ ��� ��������� ������������� ��������
        /// </summary>
        Driver AddOrUpdateDriver(Guid agentGuid, Driver driver);

        /// <summary>
        /// ����� ������� ������������� ��������
        /// </summary>
        void DeleteDriver(Guid agentGuid, Driver driver);

        /// <summary>
        /// ����� ���������� ������ ��������� �� ���������������
        /// </summary>
        /// <returns></returns>
        IList<Driver> GetDriversByGuids(Guid agentGuid, IList<Guid> guids);
    }
}