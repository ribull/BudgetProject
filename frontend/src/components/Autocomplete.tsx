import React, { useState } from 'react';
import { Form } from 'react-bulma-components';
import { Color } from 'react-bulma-components/src/components';

const { Input, Control } = Form;

interface AutoCompleteProps {
  value: string;
  possibleMatches: string[];
  color: Color;
  onSelection: (selection: string, autoCompleted: boolean) => void;
}

export default function AutoComplete({
  value,
  possibleMatches,
  color,
  onSelection,
}: AutoCompleteProps) {
  const [matches, setMatches] = useState<string[]>([]);
  const [selectedIndex, setSelectedIndex] = useState<number>();

  function handleTyping(event: React.ChangeEvent<HTMLInputElement>) {
    onSelection(event.currentTarget.value, false);
    setMatches(
      event.currentTarget.value === ''
        ? []
        : possibleMatches.filter((match) =>
            match.includes(event.currentTarget.value),
          ),
    );

    setSelectedIndex(undefined);
  }

  function handleKeyPress(event: React.KeyboardEvent<HTMLInputElement>) {
    switch (event.key) {
      case 'Enter': {
        if (selectedIndex !== undefined) {
          onSelection(matches[selectedIndex], true);
        }

        setMatches([]);
        setSelectedIndex(undefined);
        break;
      }
      case 'ArrowDown': {
        const selection = selectedIndex === undefined ? 0 : selectedIndex + 1;
        setSelectedIndex(selection === matches.length ? 0 : selection);
        break;
      }
      case 'ArrowUp': {
        const selection = selectedIndex === undefined ? 0 : selectedIndex - 1;
        setSelectedIndex(selection < 0 ? 0 : selection);
        break;
      }
      default:
        break;
    }
  }

  function handleSelection(event: React.MouseEvent, selectedMatch: string) {
    onSelection(selectedMatch, true);
    setSelectedIndex(undefined);
    setMatches([]);
  }

  return (
    <Control>
      <div className={`dropdown ${matches.length > 0 ? 'is-active' : ''}`}>
        <div className="dropdown-trigger">
          <Input
            type="text"
            value={value}
            onChange={(event) => handleTyping(event)}
            onKeyDown={(event) => handleKeyPress(event)}
            color={color}
          />
        </div>
        <div className="dropdown-menu">
          {matches.length > 0 && (
            <div className="dropdown-content">
              {matches.map((match, index) => (
                // eslint-disable-next-line jsx-a11y/anchor-is-valid
                <a
                  className={`dropdown-item ${
                    selectedIndex === index ? 'is-active' : ''
                  }`}
                  key={match}
                  onClick={(event) => handleSelection(event, match)}
                  href="#"
                >
                  {match}
                </a>
              ))}
            </div>
          )}
        </div>
      </div>
    </Control>
  );
}
