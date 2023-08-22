import clsx from "clsx";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { signIn, signOut, useSession } from "next-auth/react";
import { Button } from "./ui/Button";
import { useState } from "react";

export function Navbar() {
  const { status } = useSession();
  const [isOpen, setIsOpen] = useState(false);
  const close = () => setIsOpen(false);

  return (
    <div className="fixed top-0 z-10 flex w-full flex-col border-b border-gray-800 bg-black lg:bottom-0 lg:z-auto lg:w-72 lg:border-b-0 lg:border-r lg:border-gray-800">
      <div className="flex h-14 items-center p-4 lg:h-auto">
        <Link href="/" className="group flex w-full items-center gap-x-2.5" onClick={close}>
          <h3 className="font-semibold tracking-wide text-gray-400 group-hover:text-gray-50">Example</h3>
        </Link>
      </div>
      <button
        type="button"
        className="group absolute right-0 top-0 flex h-14 items-center gap-x-2 px-4 lg:hidden"
        onClick={() => setIsOpen(!isOpen)}
      >
        <div className="font-medium text-gray-100 group-hover:text-gray-400">Menu</div>
      </button>

      <div
        className={clsx("overflow-y-auto lg:static lg:block", {
          "fixed inset-x-0 bottom-0 top-14 mt-px bg-black": isOpen,
          hidden: !isOpen,
        })}
      >
        <nav className="space-y-6 px-2 pb-24 pt-5">
          <div className="space-y-1">
            <NavbarItem title="Home" link="" close={close} />
            {status === "unauthenticated" ? (
              <Button
                className="block rounded-md px-3 py-2 text-sm font-medium text-gray-400 hover:bg-gray-800 hover:text-gray-300"
                onClick={() => signIn("auth0")}
              >
                Login
              </Button>
            ) : null}

            {status === "authenticated" ? (
              <>
                <Button
                  className="block rounded-md px-3 py-2 text-sm font-medium text-gray-400 hover:bg-gray-800 hover:text-gray-300"
                  onClick={() => signOut({ callbackUrl: "/" })}
                >
                  Logout
                </Button>
              </>
            ) : null}
          </div>
        </nav>
      </div>
    </div>
  );
}

type NavbarItemProps = {
  title: string;
  link: string;
  close: () => void;
};
function NavbarItem({ link, title, close }: NavbarItemProps) {
  const path = usePathname().slice(1);
  const isActive = link === path;

  return (
    <Link
      onClick={close}
      href={`/${link}`}
      className={clsx("block rounded-md px-3 py-2 text-sm font-medium hover:bg-gray-800 hover:text-gray-300", {
        "text-gray-400 ": !isActive,
        "text-white": isActive,
      })}
    >
      {title}
    </Link>
  );
}
