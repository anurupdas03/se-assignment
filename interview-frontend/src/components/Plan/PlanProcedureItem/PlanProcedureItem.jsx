import React, { useState, useEffect } from "react";
import ReactSelect from "react-select";
import { assignUsersToProc, getAssignedUsers } from "../../../api/api";

const PlanProcedureItem = ({ procedure, users, planId }) => {
  const [selectedUsers, setSelectedUsers] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadAssignedUsers = async () => {
      try {
        setIsLoading(true);
        setError("");
        const assignedUsers = await getAssignedUsers(
          planId,
          procedure.procedureId
        );
        const selectedOptions = assignedUsers.map((user) => ({
          value: user.userId,
          label: user.name,
        }));
        setSelectedUsers(selectedOptions);
      } catch (error) {
        setError("Failed to load assigned users");
        console.error("Failed to load assigned users:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadAssignedUsers();
  }, [planId, procedure.procedureId]);

  const handleAssignUserToProcedure = async (selected) => {
    try {
      setIsLoading(true);
      setError("");
      const userIds = selected ? selected.map((s) => s.value) : [];
      await assignUsersToProc(planId, procedure.procedureId, userIds);
      setSelectedUsers(selected);
    } catch (error) {
      setError("Failed to assign users");
      console.error("Failed to assign users:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="py-2">
      <div className="d-flex justify-content-between align-items-center">
        <div>{procedure.procedureTitle}</div>
        {error && <div className="text-danger small">{error}</div>}
      </div>
      <ReactSelect
        className="mt-2"
        placeholder={isLoading ? "Loading..." : "Select Users to Assign"}
        isMulti={true}
        options={users}
        value={selectedUsers}
        onChange={handleAssignUserToProcedure}
        isDisabled={isLoading}
        isLoading={isLoading}
      />
    </div>
  );
};

export default PlanProcedureItem;
