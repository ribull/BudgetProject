import { useCallback, useEffect, useState } from 'react';
import { ElectronHandler } from '../../main/preload';
import { isWishlist } from '../../helpers/TypeSafety';
import { WishlistItem } from '../../generated/budget_service';
import InsertWishlistItem from './InsertWishlistItem';
import WishlistTable from './WishlistTable';

interface WishlistProps {
  isConnected: boolean;
  contextBridge: ElectronHandler;
}

export default function Wishlist({
  isConnected,
  contextBridge,
}: WishlistProps) {
  const [wishlist, setWishlist] = useState<WishlistItem[]>([]);

  async function addWishlistItem(
    description: string,
    amount: number,
    notes: string,
  ) {
    await contextBridge.ipcRenderer.invoke('add-wishlist-item', [
      description,
      amount,
      notes,
    ]);
  }

  const editWishlistItem = useCallback(
    async (
      wishlistItemId: number,
      description: string,
      amount: number,
      notes: string,
    ) => {
      await contextBridge.ipcRenderer.invoke('edit-wishlist-item', [
        wishlistItemId,
        description,
        amount,
        notes,
      ]);
    },
    [contextBridge],
  );

  const getWishlist = useCallback(async (): Promise<WishlistItem[]> => {
    if (isConnected) {
      const wishlistResp =
        await contextBridge.ipcRenderer.invoke('get-wishlist');
      if (isWishlist(wishlistResp)) {
        return wishlistResp;
      }
    }

    return [];
  }, [contextBridge, isConnected]);

  const refreshWishlistData = useCallback(() => {
    console.log('refreshing...');

    getWishlist()
      .then((wishlistRes) => setWishlist(wishlistRes))
      .catch((err) => console.log(`Couldn't retrieve wishlist: ${err}`));
  }, [getWishlist]);

  useEffect(() => {
    if (isConnected) {
      refreshWishlistData();
    }
  }, [contextBridge, refreshWishlistData, isConnected]);

  return (
    <div>
      <InsertWishlistItem
        onSubmit={(description, amount, notes) =>
          addWishlistItem(description, amount, notes)
            .then(() => refreshWishlistData())
            .catch((err) =>
              console.log(
                `An error occured while inserting an investment: ${err}`,
              ),
            )
        }
      />
      <WishlistTable
        maxRows={15}
        wishlist={wishlist}
        saveEditedWishlistItem={(wishlistItemId, description, amount, notes) =>
          editWishlistItem(wishlistItemId, description, amount, notes)
            .then(() => refreshWishlistData())
            .catch((err) =>
              console.log(
                `An error occured while editing a wishlist item: ${err}`,
              ),
            )
        }
        deleteRow={(wishlistItemId) =>
          contextBridge.ipcRenderer
            .invoke('delete-wishlist-item', wishlistItemId)
            .then(() => refreshWishlistData())
            .catch((err) => console.log(err))
        }
      />
    </div>
  );
}
