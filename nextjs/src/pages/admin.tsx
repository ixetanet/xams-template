import { AdminDashboard } from "@ixeta/xams";
import React from "react";
import { Avatar, Box, Button, Divider, Loader, NavLink } from "@mantine/core";
import { IconChevronRight, IconLogout } from "@tabler/icons-react";
import { useRouter } from "next/router";
import { useAuthProtect } from "@ixeta/xams-firebase";

const Admin = () => {
  const router = useRouter();
  const auth = useAuthProtect();

  if (auth.isLoading || !router.isReady) {
    return (
      <div className="w-full h-full flex justify-center items-center">
        <Loader />
      </div>
    );
  }

  if (auth.isError) {
    return <div>Error loading auth settings</div>;
  }

  if (!auth.isLoggedIn) {
    localStorage.setItem("postLoginRedirect", router.asPath);
    router.push("/login");
    return <></>;
  }

  return (
    <AdminDashboard
      addMenuItems={[
        {
          order: 10000,
          navLink: (
            <NavLink
              label="Logout"
              leftSection={<IconLogout size={16} />}
              onClick={() => {
                auth.signOut();
                router.push("/");
              }}
            />
          ),
        },
      ]}
      userCard={
        <Box>
          <Divider />
          <NavLink
            label={auth.firebaseAuth?.currentUser?.email || "User"}
            leftSection={<Avatar size="sm" />}
            rightSection={<IconChevronRight size={14} stroke={1.5} />}
            onClick={() => router.push("/profile")}
            p="md"
          />
        </Box>
      }
      accessDeniedMessage={
        <div className="w-full h-full flex flex-col gap-2 justify-center items-center">
          You don&apos;t have permission to view this page. Please contact your
          system administrator.
          {auth.firebaseApp != null && (
            <Button
              onClick={() => {
                auth.signOut();
                router.push("/");
              }}
            >
              Logout
            </Button>
          )}
        </div>
      }
    />
  );
};

export default Admin;
