import { Box } from 'react-bulma-components';
import { useState } from 'react';
import AutoComplete from './Autocomplete';
import { ElectronHandler } from '../main/preload';

interface InsertPurchaseProps {
  contextBridge: ElectronHandler;
}

export default function InsertPurchase({ contextBridge }: InsertPurchaseProps) {
  const [value, setValue] = useState<string>('');

  const matches = [
    'hello',
    'hi',
    'hydrogen',
    'hell',
    'banana',
    'bastard',
    'bat',
    'carrot',
  ];

  return (
    <Box>
      <AutoComplete
        value={value}
        label="test"
        possibleMatches={matches}
        onSelection={(selection) => {
          setValue(selection);
        }}
      />
    </Box>
  );
}
