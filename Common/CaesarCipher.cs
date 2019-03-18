using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace space_engineers.Common
{
   public class CaesarCipher
   {
      private String m_alphabet  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_| "                           ;
      private String m_secretKey = "578421324877894231878613219875123123187532187683218646512184511657";

      private Dictionary<char, int> m_dictAlphabet = new Dictionary<char, int> ();

      //------------------------------------------------------------------------

      public CaesarCipher ()
      {
         for ( int nIndex = 0; nIndex < m_alphabet.Length; nIndex++ )
         {
            m_dictAlphabet.Add ( m_alphabet  [nIndex], nIndex );
         }

         // sender|receiver|signature|signaturevalue|messagetype

         String plainText = "ship_01|ship_02|5000|1000|startassembling".ToUpper ();

         int[] shiftBuffer = GetShiftBuffer ( plainText, m_secretKey );

         String encryptedText = CaesarEncrypt ( plainText, shiftBuffer );

         String plainTextTest = CaesarDecrypt ( encryptedText, shiftBuffer );
      }

      private String CaesarEncrypt ( String plainText, int[] shiftBuffer )
      {
         StringBuilder encryptedText = new StringBuilder ( plainText.Length );

         for ( int nIndex = 0; nIndex < plainText.Length; nIndex++ )
         {
            char c = plainText[nIndex];

            int nCharIndex = m_dictAlphabet[c];

            int nEncryptedIndex = ( nCharIndex + shiftBuffer[nIndex] ) % m_alphabet.Length;

            encryptedText.Append ( m_alphabet[nEncryptedIndex] );
         }

         return encryptedText.ToString ();
      }

      private String CaesarDecrypt ( String encryptedText, int[] shiftBuffer )
      {
         StringBuilder plainText = new StringBuilder ( encryptedText.Length );

         for ( int nIndex = 0; nIndex < encryptedText.Length; nIndex++ )
         {
            char c = encryptedText[nIndex];

            int nCharIndex = m_dictAlphabet[c];

            int nEncryptedIndex = ( nCharIndex - shiftBuffer[nIndex] ) % m_alphabet.Length;

            if ( nEncryptedIndex < 0 )
            {
               nEncryptedIndex = m_alphabet.Length + nEncryptedIndex;
            }

            plainText.Append ( m_alphabet[nEncryptedIndex] );
         }

         return plainText.ToString ();
      }

      private int[] GetShiftBuffer ( String plainText, String secretKey )
      {
         int[] shiftBuffer = new int[plainText.Length];

         for ( int nIndex = 0; nIndex < plainText.Length; nIndex++ )
         {
            if ( nIndex < secretKey.Length )
            {
               int shift;

               int.TryParse ( secretKey[nIndex] + "", out shift );

               shiftBuffer[nIndex] = shift;
            }
            else
            {
               char c = plainText[nIndex];

               if ( m_dictAlphabet.ContainsKey ( c ))
               {
                  shiftBuffer[nIndex] = m_dictAlphabet[c];
               }
               else
               {
                  shiftBuffer[nIndex] = 0;
               }
            }
         }

         return shiftBuffer;
      }
   }
}
