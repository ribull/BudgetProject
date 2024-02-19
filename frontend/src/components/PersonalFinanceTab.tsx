import { Block, Columns, Menu } from 'react-bulma-components';
import { useState } from 'react';
import { ElectronHandler } from '../main/preload';
import PurchaseHistory from './PurchaseHistory/PurchaseHistory';
import PayHistoryHistory from './PayHistory/PayHistoryHistory';
import FuturePurchases from './FuturePurchase/FuturePurchases';
import Investments from './Investments/Investments';
import Savings from './Savings/Savings';
import Wishlist from './Wishlist/Wishlist';

enum PersonalFinanceSections {
  Record = 'Record',
  Visualize = 'Visualize',
}

interface SelectedSection {
  section: PersonalFinanceSections;
  subsection: string;
}

interface PersonalFinanceTabProps {
  contextBridge: ElectronHandler;
  isConnected: boolean;
}

export default function PersonalFinanceTab({
  isConnected,
  contextBridge,
}: PersonalFinanceTabProps) {
  const [selectedSection, setSelectedSection] = useState<SelectedSection>();

  const sections: Record<PersonalFinanceSections, string[]> = {
    [PersonalFinanceSections.Record]: [
      'Purchase History',
      'Future Purchase',
      'Pay History',
      'Investments',
      'Savings',
      'Wishlist'
    ],
    [PersonalFinanceSections.Visualize]: ['Tables', 'Graphs'],
  };

  return (
    <Columns>
      <Columns.Column narrow>
        <Menu>
          <Menu.List title="Personal Finance">
            {Object.values(PersonalFinanceSections).map((section) => (
              <Menu.List.Item>
                <Menu.List title={section}>
                  {sections[section].map((subsection) => (
                    <Menu.List.Item
                      onClick={() =>
                        setSelectedSection({ section, subsection })
                      }
                      active={
                        selectedSection?.section === section &&
                        selectedSection?.subsection === subsection
                      }
                    >
                      {subsection}
                    </Menu.List.Item>
                  ))}
                </Menu.List>
              </Menu.List.Item>
            ))}
          </Menu.List>
        </Menu>
      </Columns.Column>
      <Columns.Column>
        <Block>
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Purchase History' && (
              <PurchaseHistory contextBridge={contextBridge} isConnected={isConnected} />
            )}
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Future Purchase' && (
              <FuturePurchases
                contextBridge={contextBridge}
                isConnected={isConnected}
              />
            )}
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Pay History' && (
              <PayHistoryHistory
                contextBridge={contextBridge}
                isConnected={isConnected}
              />
            )}
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Investments' && (
              <Investments
                contextBridge={contextBridge}
                isConnected={isConnected}
              />
            )}
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Savings' && (
              <Savings
                contextBridge={contextBridge}
                isConnected={isConnected}
              />
            )}
          {selectedSection?.section === PersonalFinanceSections.Record &&
            selectedSection.subsection === 'Wishlist' && (
              <Wishlist
                contextBridge={contextBridge}
                isConnected={isConnected}
              />
            )}
        </Block>
      </Columns.Column>
    </Columns>
  );
}
